using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EstiloMB.Site.Controllers
{
    public class ProdutoController : Controller
    {
        [Authorize]
        public IActionResult Details()
        {
            return View();
        }
    }
}
