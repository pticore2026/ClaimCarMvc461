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

        public ActionResult Lookup(string policyNumber)
        {
            var policy=string.IsNullOrWhiteSpace(policyNumber)?null:_service.Repository.GetVehiclePolicy(policyNumber.Trim());
            if(policy==null)return Json(new{found=false},JsonRequestBehavior.AllowGet);
            return Json(new{found=true,policy.InsuredAmount,policy.VehicleValue,policy.Deductible,policy.Brand,policy.Model,policy.ChassisNumber},JsonRequestBehavior.AllowGet);
        }
        public ActionResult PopupByPolicyNumber(string policyNumber)
        {
            var policy=string.IsNullOrWhiteSpace(policyNumber)?null:_service.Repository.GetVehiclePolicy(policyNumber.Trim());
            return View("Popup",new PolicyDetailViewModel{Policy=policy});
        }

        public ActionResult Popup(int claimId)
        {
            var claim=_service.Repository.Get(claimId);
            if(claim==null)return HttpNotFound();
            return View(new PolicyDetailViewModel{Claim=claim,Policy=_service.Repository.GetVehiclePolicy(claim.PolicyNumber)});
        }
    }
}
