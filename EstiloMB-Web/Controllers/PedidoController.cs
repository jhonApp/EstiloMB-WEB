using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EstiloMB.Core;
using System.Collections.Generic;
using EstiloMB.MVC;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EstiloMB.Site.Controllers
{
    public class PedidoController : Controller
    {
        
        public IActionResult GerarPedido(int produtoID, string tamanho, int usuarioID, int corID)
        {
            usuarioID = 3;
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();

            Pedido pedido = Pedido.GetByUsuarioID(usuarioID);

            if (pedido == null)
            {
                pedido = Pedido.CreatePedido(usuarioID);
            }

            List<ItemPedido> itemPedidos = ItemPedido.GerarItemPedido(pedido, produtoID, tamanho, corID);

            response.Total = itemPedidos.Count();
            response.Data = itemPedidos;
            response.Code = ResponseCode.Sucess;

            return Json(response.Data);
        }

        [HttpPost]
        public IActionResult GetPedido(int pedidoID)
        {
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();
            List<ItemPedido> itemPedidos = ItemPedido.GetByPedido(pedidoID);

            response.Data = itemPedidos;
            return Json(response.Data);
        }

        public IActionResult RemoveItem(int ID, int pedidoID)
        {
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();
            ItemPedido.Remove(ID);

            List<ItemPedido> itemPedidos = ItemPedido.GetByPedido(pedidoID);

            if(itemPedidos.Count == 0)
            {
                Pedido.Remove(pedidoID);
            }
            
            response.Data = itemPedidos;
            return Json(response.Data);
        }

    }
}
