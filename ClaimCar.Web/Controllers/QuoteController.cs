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
        public ActionResult Edit(int claimId){ViewBag.Claim=_service.Repository.Get(claimId);if(ViewBag.Claim==null)return HttpNotFound();return View(_service.Repository.GetQuote(claimId));}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(QuoteViewModel m){ViewBag.Claim=_service.Repository.Get(m.ClaimId);m.Items=m.Items??new System.Collections.Generic.List<QuoteItem>();m.RepairTotal=m.Items.Sum(x=>x.PartPrice);m.PaintTotal=m.Items.Sum(x=>x.PaintCost);m.LaborTotal=m.Items.Sum(x=>x.LaborCost);m.CustomerPaymentTotal=m.DeductibleAmount;var gross=m.ReplacementTotal+m.SpecialReplacementTotal+m.RepairTotal+m.PaintTotal+m.LaborTotal+m.TowingTotal;var discount=(m.RepairTotal*m.RepairDiscountPercent/100m)+(m.PaintTotal*m.PaintDiscountPercent/100m)+(m.ReplacementTotal*m.ReplacementDiscountPercent/100m);m.ApprovedTotal=Math.Max(0,gross-discount-m.CustomerPaymentTotal);if(!ModelState.IsValid)return View(m);try{var e=_service.SaveQuote(m);if(e!=null){ModelState.AddModelError("",e);return View(m);}TempData["Success"]="Đã tính và lưu báo giá.";return RedirectToAction("Edit",new{claimId=m.ClaimId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(m);}}
    }
}
