using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using WebAppDistributedSignOutDotNet.App_Start;
using System.Web;
using System.Web.Mvc;

namespace WebAppDistributedSignOutDotNet.Controllers
{
    public class AccountController : Controller
    {
        // Action is used for constructing the OpenIDConnect request that the javascript
        // will use to check if the user has been logged out of AAD.
        [Authorize]
        public JsonResult SessionChanged()
        {
            // If the javascript made the reuest, issue a challenge so the OIDC request will be constructed.
            if (HttpContext.GetOwinContext().Request.QueryString.Value == "")
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/Account/SessionChanged" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
                return Json(new { }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                // 'RedirectToIdentityProvider' redirects here with the OIDC request as the query string
                return Json(HttpContext.GetOwinContext().Request.QueryString.Value, "application/json", JsonRequestBehavior.AllowGet);
            }
        }

        // Action for displaying a page notifying the user that they've been signed out automatically.
        public ActionResult SingleSignOut(string redirectUri)
        {
            // RedirectUri is necessary to bring a user back to the same location 
            // if they re-authenticate after a single sign out has occurred. 
            if (redirectUri == null)
                ViewBag.RedirectUri = "https://localhost:44308/";
            else
                ViewBag.RedirectUri = redirectUri;

            // We need to sign the user out of the Application only,
            // because they have already been logged out of AAD
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return View();
        }

        // Sign in has been triggered from Sign In Button or From Single Sign Out Page.
        public void SignIn(string redirectUri)
        {
            // RedirectUri is necessary to bring a user back to the same location 
            // if they re-authenticate after a single sign out has occurred.
            if (redirectUri == null)
                redirectUri = "/";
            if (!Request.IsAuthenticated)
            {
                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = redirectUri },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
        }

        public void EndSession()
        {
            Request.GetOwinContext().Authentication.SignOut();
            Request.GetOwinContext().Authentication.SignOut(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ApplicationCookie);
            this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
        }

        // Sign a user out of both AAD and the Application
        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = OwinStartup.PostLogoutRedirectUri },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
        }
    }
}