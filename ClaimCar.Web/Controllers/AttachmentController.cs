using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;

namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public class AttachmentController : Controller
    {
        private const int ImagesPerPage = 10;
        private readonly ClaimService _service = new ClaimService();
        private static readonly IList<AttachmentCategory> Categories = new List<AttachmentCategory>
        {
            new AttachmentCategory { Code="scene", Name="Ảnh hiện trường" }, new AttachmentCategory { Code="garage", Name="Ảnh Gara" },
            new AttachmentCategory { Code="vehicle", Name="Hồ sơ xe" }, new AttachmentCategory { Code="casco", Name="Thiệt hại sửa chữa VCX" },
            new AttachmentCategory { Code="cargo", Name="Thiệt hại hàng hóa" }, new AttachmentCategory { Code="third-party", Name="Thiệt hại bên thứ ba" },
            new AttachmentCategory { Code="authority", Name="Hồ sơ cơ quan chức năng" }, new AttachmentCategory { Code="driver-passenger", Name="Thiệt hại về NNTX" },
            new AttachmentCategory { Code="repair", Name="Chứng từ sửa chữa" }, new AttachmentCategory { Code="other-image", Name="Ảnh khác" },
            new AttachmentCategory { Code="other-document", Name="Tài liệu khác" }
        };

        public ActionResult Index(int? claimId, string type, string sort, int page=1)
        {
            var claims=_service.Repository.Search(null,null);
            var claim=claimId.HasValue?_service.Repository.Get(claimId.Value):claims.FirstOrDefault();
            if(claimId.HasValue && claim==null)return HttpNotFound();
            type=IsCategory(type)?type:"all"; sort=sort=="name-asc"||sort=="name-desc"?sort:"default";
            var files=claim==null?new List<AttachmentItem>():ReadFiles(claim.Id,type,sort);
            var allImages=files.Where(x=>x.IsImage).ToList();
            var totalPages=Math.Max(1,(int)Math.Ceiling(allImages.Count/(double)ImagesPerPage)); page=Math.Max(1,Math.Min(page,totalPages));
            return View(new AttachmentViewModel{ClaimId=claim==null?(int?)null:claim.Id,ClaimNumber=claim==null?null:claim.ClaimNumber,
                Type=type,Sort=sort,Page=page,TotalPages=totalPages,Claims=claims,Categories=Categories,Files=files,
                Images=allImages.Skip((page-1)*ImagesPerPage).Take(ImagesPerPage).ToList()});
        }

        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Upload(int claimId,string type,HttpPostedFileBase file)
        {
            if(_service.Repository.Get(claimId)==null)return HttpNotFound();
            if(!IsCategory(type)||type=="all")TempData["AttachmentError"]="Vui lòng chọn loại chứng từ.";
            else if(file==null||file.ContentLength==0)TempData["AttachmentError"]="Vui lòng chọn file cần tải lên.";
            else if(file.ContentLength>20*1024*1024)TempData["AttachmentError"]="Dung lượng file không được vượt quá 20 MB.";
            else { var directory=CategoryDirectory(claimId,type);Directory.CreateDirectory(directory);var name=Path.GetFileName(file.FileName);
                if(string.IsNullOrWhiteSpace(name))TempData["AttachmentError"]="Tên file không hợp lệ.";
                else{file.SaveAs(UniquePath(directory,name));TempData["Success"]="Đã tải chứng từ lên.";} }
            return RedirectToAction("Index",new{claimId=claimId,type=type,sort="default",page=1});
        }

        public ActionResult ViewFile(int claimId,string type,string fileName)
        {
            var claim=_service.Repository.Get(claimId);if(claim==null)return HttpNotFound();var item=FindFile(claimId,type,fileName);if(item==null)return HttpNotFound();
            return View(new AttachmentPreviewViewModel{ClaimId=claimId,ClaimNumber=claim.ClaimNumber,File=item});
        }
        public ActionResult Content(int claimId,string type,string fileName)
        {
            var item=FindFile(claimId,type,fileName);if(item==null)return HttpNotFound();return File(Path.Combine(CategoryDirectory(claimId,type),item.FileName),item.ContentType);
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Delete(int claimId,string type,string fileName,string returnType,string sort,int page=1)
        {
            if(_service.Repository.Get(claimId)==null)return HttpNotFound();
            var item=FindFile(claimId,type,fileName);if(item==null)return HttpNotFound();
            try
            {
                System.IO.File.Delete(Path.Combine(CategoryDirectory(claimId,type),item.FileName));
                TempData["Success"]="Đã xóa file \""+item.FileName+"\".";
            }
            catch(IOException){TempData["AttachmentError"]="Không thể xóa file. Vui lòng thử lại.";}
            catch(UnauthorizedAccessException){TempData["AttachmentError"]="Không có quyền xóa file.";}
            return RedirectToAction("Index",new{claimId=claimId,type=IsCategory(returnType)?returnType:"all",sort=sort,page=page});
        }
        private List<AttachmentItem> ReadFiles(int claimId,string type,string sort)
        {
            var selected=type=="all"?Categories:Categories.Where(x=>x.Code==type).ToList();var files=new List<AttachmentItem>();
            foreach(var category in selected){var directory=CategoryDirectory(claimId,category.Code);if(!Directory.Exists(directory))continue;
                foreach(var path in Directory.GetFiles(directory)){var info=new FileInfo(path);var contentType=MimeType(info.Extension);files.Add(new AttachmentItem{FileName=info.Name,CategoryCode=category.Code,CategoryName=category.Name,ContentType=contentType,Size=info.Length,UploadedAt=info.LastWriteTime,IsImage=contentType.StartsWith("image/",StringComparison.OrdinalIgnoreCase)});}}
            if(sort=="name-asc")return files.OrderBy(x=>x.FileName,StringComparer.CurrentCultureIgnoreCase).ToList();
            if(sort=="name-desc")return files.OrderByDescending(x=>x.FileName,StringComparer.CurrentCultureIgnoreCase).ToList();return files.OrderByDescending(x=>x.UploadedAt).ToList();
        }
        private AttachmentItem FindFile(int claimId,string type,string fileName)
        {
            if(!IsCategory(type)||type=="all"||string.IsNullOrWhiteSpace(fileName)||Path.GetFileName(fileName)!=fileName)return null;
            return ReadFiles(claimId,type,"default").FirstOrDefault(x=>string.Equals(x.FileName,fileName,StringComparison.Ordinal));
        }
        private static bool IsCategory(string type){return type=="all"||Categories.Any(x=>x.Code==type);}
        private string CategoryDirectory(int claimId,string type){return Server.MapPath("~/App_Data/Attachments/"+claimId+"/"+type);}
        private static string UniquePath(string directory,string fileName){var path=Path.Combine(directory,fileName);if(!System.IO.File.Exists(path))return path;var stem=Path.GetFileNameWithoutExtension(fileName);var extension=Path.GetExtension(fileName);var i=1;do{path=Path.Combine(directory,stem+" ("+i+")"+extension);i++;}while(System.IO.File.Exists(path));return path;}
        private static string MimeType(string extension)
        {
            switch((extension??"").ToLowerInvariant()){case ".jpg":case ".jpeg":return "image/jpeg";case ".png":return "image/png";case ".gif":return "image/gif";case ".webp":return "image/webp";case ".bmp":return "image/bmp";case ".pdf":return "application/pdf";case ".txt":return "text/plain";case ".csv":return "text/csv";case ".doc":return "application/msword";case ".docx":return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";case ".xls":return "application/vnd.ms-excel";case ".xlsx":return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";default:return "application/octet-stream";}
        }
    }
}
