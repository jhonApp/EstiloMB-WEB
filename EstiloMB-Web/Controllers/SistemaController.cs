using Microsoft.AspNetCore.Mvc;

namespace EstiloMB.Site.Controllers
{
    public class SistemaController : Controller
    {
        public IActionResult login()
        {
            return View();
        }

        public IActionResult CriarConta()
        {
            return View();
        }
    }
}
