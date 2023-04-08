﻿using Microsoft.AspNetCore.Mvc;
using EstiloMB.Core;
using EstiloMB.MVC;

namespace EstiloMB.Site.Controllers
{
    public class PagamentoController : Controller
    {
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

        public int UserValidation()
        {
            int userID = User.GetClaimInt32("UsuarioID");
            return userID;
        }

        public string GetCreditCardBrand(string number)
        {
            string bandeira = Pedido.GetCardBrand(number) ?? "Padrão";
            return bandeira;
        }

        public bool ValidaCreditCard(string number)
        {
            bool isValid = Pedido.IsValidCreditCardNumber(number);

            return isValid;
        }

        public IActionResult Recusado()
        {
            return View();
        }

        public IActionResult Aprovado()
        {
            return View();
        }

    }
}
