using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstiloMB.Core;
using System.Collections.Generic;
using EstiloMB.MVC;

namespace EstiloMB.Site.Controllers
{
    public class ProdutoController : Controller
    {
        public IActionResult Details(int produtoID)
        {
            ViewBag.ProdutoID = produtoID;
            return View();
        }

        public IActionResult Sacola(int pedidoID)
        {
            ViewBag.PedidoID = pedidoID;
            return View();
        }

        public IActionResult SacolaVazia()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetProdutoByID(int ID)
        {
            Response<Produto> response = new Response<Produto>();

            response.Data = Produto.GetByID(ID);
            return Json(response);
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

        [HttpPost]
        public IActionResult RemoveImagem(int ID)
        {
            Response<ProdutoImagem> response = ProdutoImagem.Remove(ID);
            return Json(response);
        }
    }
}
