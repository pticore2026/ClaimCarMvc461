using System.Web.Mvc;
using ClaimCar.Web.Models;
using ClaimCar.Web.Services;

namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public sealed class PolicyController : Controller
    {
        private readonly ClaimService _service=new ClaimService();

        public ActionResult Details(int claimId)
        {
            var claim=_service.Repository.Get(claimId);
            if(claim==null)return HttpNotFound();
            ViewBag.ClaimId=claimId;
            return View(new PolicyDetailViewModel{Claim=claim,Policy=_service.Repository.GetVehiclePolicy(claim.PolicyNumber)});
        }

        public ActionResult Popup(int claimId)
        {
            var claim=_service.Repository.Get(claimId);
            if(claim==null)return HttpNotFound();
            return View(new PolicyDetailViewModel{Claim=claim,Policy=_service.Repository.GetVehiclePolicy(claim.PolicyNumber)});
        }
    }
}
