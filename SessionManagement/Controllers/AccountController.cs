using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using SessionManagement.App_Start;
using System.Web;
using System.Web.Mvc;

namespace SessionManagement.Controllers
{
    public class AccountController : Controller
    {
        [Authorize]
        public void SessionChanged()
        {
            // issue a challenge so that OpenIdConnectHandler will be used to create the message
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Account/SessionChanged" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            return;
        }

        [Authorize]
        public JsonResult SessionChangedCallback()
        {
            // 'RedirectToIdentityProvider' redirects here with the OIDC authorize request
            string redirectUrl =  HttpContext.GetOwinContext().Request.QueryString.Value;
            return Json(redirectUrl, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DistSignOut(string redirectUri)
        {
            if (redirectUri == null)
                ViewBag.RedirectUri = "https://localhost:44308/";
            else
                ViewBag.RedirectUri = redirectUri;

            // Sign the user out of the app, since they've already been signed out of AAD
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return View();
        }

        // sign in triggered from the Sign In gesture in the UI or from link in Distributed Sign Out page.
        public void SignIn(string redirectUri)
        {
            if (redirectUri == null)
                redirectUri = "/";

            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }
        // sign out triggered from the Sign Out gesture in the UI, so sign out of app and AAD
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = OwinStartup.PostLogoutRedirectUri }, OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}