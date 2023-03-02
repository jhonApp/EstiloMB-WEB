using Microsoft.AspNetCore.Mvc;
using EstiloMB.Core;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using ServiceReferenceCorreios;
using System;
using static ServiceReferenceCorreios.CalcPrecoPrazoWSSoapClient;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Antiforgery;

namespace EstiloMB.Site.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IDistributedCache _cache;

        public PedidoController(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<IActionResult> GerarPedido(int produtoID, string tamanho, int usuarioID, int corID)
        {
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();

            if (usuarioID != 0)
            {
                Pedido pedido = Pedido.GetByUsuarioID(usuarioID);

                if (pedido == null)
                {
                    pedido = Pedido.CreatePedido();
                }

                List<ItemPedido> itemPedidos = ItemPedido.GerarItemPedido(pedido, produtoID, tamanho, corID);

                //Removendo o pedido que está em cache
                await _cache.RemoveAsync($"Pedido:{pedido.ID}");

                response.Total = itemPedidos.Count();
                response.Data = itemPedidos;
                response.Code = ResponseCode.Sucess;
            }
            else
            {
                var cacheKey = $"PedidoID:{produtoID}:{tamanho}:{corID}";
                var cachedResult = await _cache.GetStringAsync(cacheKey);

                if (cachedResult != null)
                {
                    // Se estiver em cache, retorna o resultado armazenado
                    Pedido pedido = System.Text.Json.JsonSerializer.Deserialize<Pedido>(cachedResult);

                    response.Total = pedido.itemPedidos.Count();
                    response.Data = pedido.itemPedidos;
                }
                else
                {
                    ProdutoImagem produtoImagem = ProdutoImagem.GetByCorID(corID);
                    Pedido pedido = Pedido.CreatePedido();

                    var itemPedido = new ItemPedido
                    {
                        ProdutoID = produtoID,
                        Pedido = pedido,
                        Tamanho = tamanho,
                        Cor = produtoImagem.Cor.Nome,
                        CorID = corID,
                        Quantidade = 1
                    };

                    pedido.itemPedidos.Add(itemPedido);

                    response.Total = 1;
                    response.Data = new List<ItemPedido> { itemPedido };

                    // Armazena em cache o pedido recém-criado
                    await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(pedido));
                }
            }

            return Json(response.Data);
        }

        public IActionResult AdicionarItemAoCarrinho(int produtoID, string tamanho, int corID)
        {
            try
            {
                // Recupera o carrinho de compras do cookie
                List<ItemPedido> carrinho = ObterCarrinhoDeCompras();

                // Recupera o produto e a imagem da cor do produto
                var produto = Produto.GetByID(produtoID);
                var produtoImagem = ProdutoImagem.GetByCorID(corID);

                // Verifica se o produto já está no carrinho
                var itemPedidoExistente = carrinho.FirstOrDefault(item => item.ProdutoID == produtoID && item.Tamanho == tamanho && item.CorID == corID);
                if (itemPedidoExistente != null)
                {
                    // Se o produto já está no carrinho, aumenta a quantidade em 1
                    itemPedidoExistente.Quantidade += 1;
                }
                else
                {
                    // Se o produto ainda não está no carrinho, adiciona um novo item
                    var itemPedido = new ItemPedido
                    {
                        ProdutoID = produtoID,
                        Produto = produto,
                        Tamanho = tamanho,
                        Cor = produtoImagem.Cor.Nome,
                        CorID = corID,
                        Quantidade = 1
                    };
                    carrinho.Add(itemPedido);
                }

                // Salva o carrinho de compras no cookie
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    IgnoreReadOnlyProperties = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };
                string itemPedidosJson = System.Text.Json.JsonSerializer.Serialize(carrinho, options);

                CookieOptions cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = false,
                    SameSite = SameSiteMode.Strict,
                    IsEssential = true,
                    Secure = true
                };

                HttpContext.Session.SetString("carrinho", itemPedidosJson);

                // Retorna um objeto JSON com a lista de itens do carrinho
                return Json(carrinho);
            }
            catch (Exception ex)
            {
                // Tratamento de erro genérico
                return BadRequest(new { message = "Ocorreu um erro ao adicionar o item ao carrinho de compras." });
            }
        }

        private List<ItemPedido> ObterCarrinhoDeCompras()
        {
            List<ItemPedido> itemPedidos = new List<ItemPedido>();
            var carrinhoJson = HttpContext.Session.GetString("carrinho");


            if (carrinhoJson != null)
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                };

                try
                {
                    itemPedidos = System.Text.Json.JsonSerializer.Deserialize<List<ItemPedido>>(carrinhoJson, options);
                }
                catch (System.Text.Json.JsonException)
                {
                    // Handle deserialization error here
                }
            }

            return itemPedidos;
        }

        [HttpPost]
        public async Task<IActionResult> GetPedidoAsync(int pedidoID)
        {
            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>();

            // Verifica se o resultado da consulta já está em cache
            string cacheKey = $"PedidoID:{pedidoID}";
            string cachedResult = await _cache.GetStringAsync(cacheKey);

            if (cachedResult != null)
            {
                // Se estiver em cache, retorna o resultado armazenado
                List<ItemPedido> itemPedidos = System.Text.Json.JsonSerializer.Deserialize<List<ItemPedido>>(cachedResult);
                response.Data = itemPedidos;
            }
            else
            {
                // Se não estiver em cache, consulta o banco de dados e armazena o resultado em cache
                List<ItemPedido> itemPedidos = ItemPedido.GetByPedido(pedidoID);
                response.Data = itemPedidos;

                string resultJson = System.Text.Json.JsonSerializer.Serialize(itemPedidos);
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                await _cache.SetStringAsync(cacheKey, resultJson, cacheOptions);
            }

            return Json(response.Data);
        }

        public IActionResult GetPedido()
        {
            List<ItemPedido> carrinho = ObterCarrinhoDeCompras();

            if (carrinho == null)
            {
                return NoContent(); // retorna 204 No Content se o carrinho estiver vazio
            }

            Response<List<ItemPedido>> response = new Response<List<ItemPedido>>
            {
                Data = carrinho
            };

            return Json(response.Data); // retorna 200 OK com o objeto JSON contendo o carrinho de compras
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

        public async Task<JsonResult> ConsultarPrazo(string cep, string peso, decimal altura, decimal comprimento, decimal largura, decimal valorDeclarado)
        {
            const string nCdEmpresa = "";
            const string sDsSenha = "";
            const string nCdServico = "41106";
            const string sCepOrigem = "19940000";
            const string sCdMaoPropria = "N";
            const string sCdAvisoRecebimento = "N";

            const int nCdFormatoCaixa = 1;
            const decimal nVlDiametroCaixa = 0;

            var endpoint = new EndpointConfiguration();
            var wsCorreios = new CalcPrecoPrazoWSSoapClient(endpoint);

            try
            {
                var resultado = await wsCorreios.CalcPrecoPrazoAsync(
                    nCdEmpresa,
                    sDsSenha,
                    nCdServico,
                    sCepOrigem,
                    cep,
                    peso, // peso total da encomenda, em quilos
                    nCdFormatoCaixa,
                    comprimento,
                    altura,
                    largura,
                    nVlDiametroCaixa,
                    sCdMaoPropria,
                    valorDeclarado,
                    sCdAvisoRecebimento
                );

                var prazoEntrega = resultado.Servicos[0].PrazoEntrega;
                var valor = resultado.Servicos[0].Valor;

                var result = new[] { valor, prazoEntrega };
                return Json(result);
            }
            catch (Exception ex)
            {
                // Tratar a exceção aqui, como por exemplo, logar o erro e retornar uma mensagem de erro apropriada.
                return Json(new[] { "Não foi possivel calcular o frete!", ex.Message });
            }
        }


    }
}
