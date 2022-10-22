using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstiloMB.Core;
using System.Collections.Generic;
using EstiloMB.MVC;

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

        [HttpPost]
        public IActionResult RemoveTamanhosByID(int ID)
        {
            Response<List<Tamanho>> response = new Response<List<Tamanho>>();

            response.Data = Tamanho.RemoveTamanhosByID(ID);
            return Json(response);
        }
    }
}
