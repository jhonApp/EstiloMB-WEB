using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstiloMB.Core;
using System.Collections.Generic;
using EstiloMB.MVC;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using StackExchange.Redis;
using System;
using EstiloMB_Web.Models;
using System.Security.Claims;

namespace EstiloMB.Site.Controllers
{
    public class ProdutoController : Controller
    {
        public IActionResult Details(int produtoID)
        {
            ViewBag.ProdutoID = produtoID;
            return View();
        }

        public IActionResult Sacola()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult FormaPagamento()
        {
            int userID = User.GetClaimInt32("UsuarioID");
            PagamentoViewModel model = new();
            model.Pedido = Pedido.GetByStatus(userID);
            return View(model);
        }

        public IActionResult GetPagamento()
        {
            int userID = User.GetClaimInt32("UsuarioID");

            Pedido pedido = Pedido.GetByStatus(userID);

            if (pedido == null)
            {
                return NoContent(); // retorna 204 No Content se o carrinho estiver vazio
            }

            Response<Pedido> response = new Response<Pedido>
            {
                Data = pedido
            };

            return Json(response.Data); // retorna 200 OK com o objeto JSON contendo o carrinho de compras
        }

        public IActionResult RemoveItem(int ID)
        {
            // Conecte-se ao servidor Redis
            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Recupera o carrinho de compras atual do banco de dados Redis
            var carrinhoJson = db.StringGet("carrinho");
            Pedido carrinho = JsonConvert.DeserializeObject<Pedido>(carrinhoJson);

            // Remove o item com o ID especificado do carrinho
            carrinho.ItemPedidos.RemoveAll(item => item.ID == ID);

            if (carrinho.ItemPedidos.Count != 0)
            {
                Pedido.AtualizaValorCarrinho(carrinho);

                // Salva o carrinho atualizado no Redis
                db.StringSet("carrinho", JsonConvert.SerializeObject(carrinho), TimeSpan.FromDays(30));
            }
            else
            {
                // Remove a chave do carrinho do banco de dados Redis
                db.KeyDelete("carrinho");
            }

            // Retorna um objeto JSON com a lista de itens do carrinho atualizada
            return Json(carrinho);
        }

        public IActionResult GetPedido()
        {
            Pedido carrinho = Pedido.ObterCarrinhoDeCompras();

            if (carrinho == null)
            {
                int userID = User.GetClaimInt32("UsuarioID");
                carrinho = Pedido.GetByStatus(userID);
            }

            Response<Pedido> response = new Response<Pedido>
            {
                Data = carrinho
            };

            return Json(response.Data); // retorna 200 OK com o objeto JSON contendo o carrinho de compras
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
