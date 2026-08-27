using System;
using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;
namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public class LossPaymentController : Controller
    {
        private readonly ClaimService _service=new ClaimService();
        public ActionResult Edit(int claimId){ViewBag.Claim=_service.Repository.Get(claimId);if(ViewBag.Claim==null)return HttpNotFound();return View(_service.Repository.GetLossPayment(claimId));}
        [HttpPost,ValidateAntiForgeryToken]
        public ActionResult Edit(LossPaymentViewModel m){ViewBag.Claim=_service.Repository.Get(m.ClaimId);if(!ModelState.IsValid)return View(m);try{var e=_service.SaveLoss(m);if(e!=null){ModelState.AddModelError("",e);return View(m);}TempData["Success"]="Đã lưu thông tin tổn thất/chi trả.";return RedirectToAction("Edit","Quote",new{claimId=m.ClaimId});}catch(Exception ex){ModelState.AddModelError("",ex.Message);return View(m);}}
    }
}
