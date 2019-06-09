using System.Web.Mvc;

namespace David.UI.Controllers
{
    public class HomeController : Controller
    {

	
		public HomeController()
		{
		}
        [Authorize]
		// GET: Home
		public ActionResult Index()
        {
            return View();
        }
    
	}
}
