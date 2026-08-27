using System;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;
namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        private readonly ClaimService _service=new ClaimService();
        public ActionResult Index(string keyword,string status){ViewBag.Keyword=keyword;ViewBag.Status=status;return View(_service.Repository.Search(keyword,status));}
        public ActionResult Create(){return View("Edit",new Claim{EntryDate=DateTime.Today,CallEntryDate=DateTime.Today,AccidentDate=DateTime.Today,NotificationDate=DateTime.Today,Status="Mới tiếp nhận",ManagementUnitCode="001",ManagementUnitName="thành phố Hồ Chí Minh",ManagementAreaCode="HCM",ManagementAreaName="Khu vực TP Hồ Chí Minh"});}
        public ActionResult Edit(int id){var x=_service.Repository.Get(id);if(x==null)return HttpNotFound();ViewBag.ClaimId=id;return View(x);}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(Claim x){if(x.Id>0)ViewBag.ClaimId=x.Id;if(ModelState.IsValid){var err=_service.ValidateClaim(x,x.Id==0?(int?)null:x.Id);if(err!=null)ModelState.AddModelError("",err);}if(!ModelState.IsValid)return View(x);try{var id=_service.SaveClaim(x);TempData["Success"]="Đã lưu thông tin chung.";return RedirectToAction("Edit",new{id=id});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(x);}}
        public ActionResult Details(int id){return RedirectToAction("Edit",new{id=id});}
        [HttpPost,ValidateAntiForgeryToken] public ActionResult Delete(int id){var x=_service.Repository.Get(id);if(x!=null)_service.DeleteClaim(x);TempData["Success"]="Đã xóa hồ sơ.";return RedirectToAction("Index");}
    }
}
