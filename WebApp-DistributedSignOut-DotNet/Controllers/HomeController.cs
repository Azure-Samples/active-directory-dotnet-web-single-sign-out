using System.Web.Mvc;

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