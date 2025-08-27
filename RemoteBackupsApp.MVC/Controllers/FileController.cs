using Microsoft.AspNetCore.Mvc;

namespace RemoteBackupsApp.MVC.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
