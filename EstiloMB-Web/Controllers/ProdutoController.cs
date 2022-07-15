using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EstiloMB.Site.Controllers
{
    public class ProdutoController : Controller
    {
        public IActionResult Details()
        {
            return View();
        }

        public IActionResult Sacola()
        {
            return View();
        }

        public IActionResult Pagamento()
        {
            return View();
        }
    }
}
