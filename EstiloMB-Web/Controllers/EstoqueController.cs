using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EstiloMB.Site.Controllers
{
    public class EstoqueController : Controller
    {

        public IActionResult Produto()
        {
            return View();
        }
    }
}
