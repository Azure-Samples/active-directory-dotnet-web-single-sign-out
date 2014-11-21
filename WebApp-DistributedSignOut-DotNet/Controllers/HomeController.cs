using System.Web.Mvc;
using System.Web;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Infrastructure;
using WebAppDistributedSignOutDotNet.App_Start;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Protocols;


//The following libraries were added to this sample.

namespace WebAppDistributedSignOutDotNet.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string error)
        {
            return View();
        }

        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        [Authorize]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}