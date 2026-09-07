using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult DraftLoss()
        {
            ViewBag.Claim=new Claim();
            return View("~/Views/LossPayment/Edit.cshtml",new LossPaymentViewModel{Coverages=new List<CoverageLine>(),OtherBeneficiaries=new List<BeneficiaryLine>(),ThirdParties=new List<ThirdPartyLine>()});
        }
        public ActionResult DraftQuote(string policyNumber)
        {
            ViewBag.Claim=new Claim();
            ViewBag.VehiclePolicy=string.IsNullOrWhiteSpace(policyNumber)?null:_service.Repository.GetVehiclePolicy(policyNumber.Trim());
            return View("~/Views/Quote/Edit.cshtml",new QuoteViewModel{Items=new List<QuoteItem>(),ParticipationValuePercent=100m,ParticipationFeePercent=100m});
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult CreateComplete([Bind(Prefix="claim")] Claim claim,[Bind(Prefix="loss")] LossPaymentViewModel loss,[Bind(Prefix="quote")] QuoteViewModel quote)
        {
            claim=claim??new Claim();loss=loss??new LossPaymentViewModel();quote=quote??new QuoteViewModel();
            claim.Id=0;claim.ClaimNumber=null;claim.Status="Mới tiếp nhận";
            foreach(var prefix in new[]{"claim.","loss.","quote."})
                if(!Request.Form.AllKeys.Any(k=>k!=null&&k.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)))
                    ModelState.AddModelError(prefix,"Vui lòng nhập đủ thông tin cả ba tab trước khi lưu.");
            if(claim.EntryDate==DateTime.MinValue||claim.AccidentDate==DateTime.MinValue||claim.NotificationDate==DateTime.MinValue)
                ModelState.AddModelError("claim.EntryDate","Vui lòng nhập đầy đủ các ngày bắt buộc.");
            var policy=string.IsNullOrWhiteSpace(claim.PolicyNumber)?null:_service.Repository.GetVehiclePolicy(claim.PolicyNumber.Trim());
            QuoteValidation.Validate(quote,policy,ModelState,"quote.");
            if(ModelState.IsValid)
            {
                var error=_service.ValidateComplete(claim,loss,quote);
                if(error!=null)ModelState.AddModelError("",error);
            }
            if(!ModelState.IsValid)return Json(new{success=false,errors=ModelState.Where(x=>x.Value.Errors.Count>0).Select(x=>new{field=x.Key,message=string.Join(" ",x.Value.Errors.Select(e=>string.IsNullOrEmpty(e.ErrorMessage)?"Giá trị nhập không hợp lệ.":e.ErrorMessage))})});
            try
            {
                var id=_service.SaveComplete(claim,loss,quote);
                TempData["Success"]="Đã lưu đầy đủ hồ sơ bồi thường. Số hồ sơ: "+claim.ClaimNumber;
                return Json(new{success=true,url=Url.Action("Edit",new{id=id})});
            }
            catch(Exception){return Json(new{success=false,errors=new[]{new{field="",message="Không lưu được hồ sơ. Vui lòng thử lại."}}});}
        }
        public ActionResult Edit(int id){var x=_service.Repository.Get(id);if(x==null)return HttpNotFound();ViewBag.ClaimId=id;return View(x);}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(Claim x){if(x.Id==0){ModelState.AddModelError("","Vui lòng nhập đủ ba tab và bấm Lưu tại tab Báo giá.");return View(x);}if(x.Id>0)ViewBag.ClaimId=x.Id;if(ModelState.IsValid){var err=_service.ValidateClaim(x,x.Id==0?(int?)null:x.Id);if(err!=null)ModelState.AddModelError("",err);}if(!ModelState.IsValid)return View(x);try{var id=_service.SaveClaim(x);TempData["Success"]="Đã lưu thông tin chung. Số hồ sơ: "+x.ClaimNumber;return RedirectToAction("Edit",new{id=id});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(x);}}
        public ActionResult Details(int id){return RedirectToAction("Edit",new{id=id});}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Delete(int id,bool continueValid=false){return DeleteJson(_service.DeleteClaims(new[]{id},User.Identity.Name,continueValid));}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult BulkDelete(int[] ids,bool continueValid=false)
        {
            return DeleteJson(_service.DeleteClaims(ids,User.Identity.Name,continueValid));
        }
        private JsonResult DeleteJson(ClaimDeleteResult result){return Json(new{status=result.Status,message=result.Message,validCount=result.ValidCount,invalidCount=result.InvalidCount,deletedIds=result.DeletedIds,partial=result.Partial});}
    }
}
