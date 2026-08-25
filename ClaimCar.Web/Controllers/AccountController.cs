using System.Web.Mvc;
using System.Web.Security;
using ClaimCar.Web.Infrastructure;
using ClaimCar.Web.Models;
namespace ClaimCar.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ConfigLoginService _login=new ConfigLoginService();
        [AllowAnonymous] public ActionResult Login(string returnUrl){if(User.Identity.IsAuthenticated)return RedirectToAction("Index","Claim");return View(new LoginViewModel{ReturnUrl=returnUrl});}
        [HttpPost,AllowAnonymous,ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel m){if(!ModelState.IsValid)return View(m);if(!_login.Validate(m.UserName,m.Password)){ModelState.AddModelError("","Tài khoản hoặc mật khẩu không đúng.");return View(m);}FormsAuthentication.SetAuthCookie(m.UserName,false);Session["DisplayName"]=_login.DisplayName;if(Url.IsLocalUrl(m.ReturnUrl))return Redirect(m.ReturnUrl);return RedirectToAction("Index","Claim");}
        public ActionResult Logout(){FormsAuthentication.SignOut();Session.Clear();return RedirectToAction("Login");}
    }
}
