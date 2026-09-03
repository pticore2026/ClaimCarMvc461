using System.Web.Mvc;

namespace ClaimCar.Web.Controllers
{
    [Authorize]
    public sealed class SupportingDocumentController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
