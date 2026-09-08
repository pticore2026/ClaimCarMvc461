using System;
using System.Linq;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;
namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public class QuoteController : Controller
    {
        private readonly ClaimService _service=new ClaimService();
        public ActionResult Edit(int claimId)
        {
            var claim=_service.Repository.Get(claimId);
            ViewBag.Claim=claim;
            if(claim==null)return HttpNotFound();
            var policy=_service.Repository.GetVehiclePolicy(claim.PolicyNumber);
            ViewBag.VehiclePolicy=policy;
            ViewBag.ClaimId=claimId;
            var model=_service.Repository.GetQuote(claimId);
            if(policy!=null&&policy.VehicleValue>0)
                model.ParticipationValuePercent=Math.Round(policy.InsuredAmount*100m/policy.VehicleValue,2,MidpointRounding.AwayFromZero);
            return View(model);
        }
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(QuoteViewModel m)
        {
            ViewBag.Claim=_service.Repository.Get(m.ClaimId);
            VehiclePolicy policy=null;
            if(ViewBag.Claim!=null)policy=_service.Repository.GetVehiclePolicy(ViewBag.Claim.PolicyNumber);
            ViewBag.VehiclePolicy=policy;
            ViewBag.ClaimId=m.ClaimId;
            QuoteValidation.Validate(m,policy,ModelState);
            if(!ModelState.IsValid)return View(m);

            try{var e=_service.SaveQuote(m);if(e!=null){ModelState.AddModelError("",e);return View(m);}TempData["Success"]="Đã tính và lưu báo giá.";return RedirectToAction("Edit",new{claimId=m.ClaimId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(m);}
        }

    }
}
