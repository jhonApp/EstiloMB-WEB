using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EstiloMB.Site.Controllers
{
    public class CRMController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Produto()
        {
            return View();
        }
    }
}
