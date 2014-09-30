using System.Web.Mvc;

namespace SessionManagement.Controllers
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
            HttpContext.Response.AddHeader("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}