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
using StackExchange.Redis;
using EstiloMB.MVC;

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

                    response.Total = pedido.ItemPedidos.Count();
                    response.Data = pedido.ItemPedidos;
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
                        NomeCor = produtoImagem.Cor.Nome,
                        CorID = corID,
                        Quantidade = 1
                    };

                    pedido.ItemPedidos.Add(itemPedido);

                    response.Total = 1;
                    response.Data = new List<ItemPedido> { itemPedido };

                    // Armazena em cache o pedido recém-criado
                    await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(pedido));
                }
            }

            return Json(response.Data);
        }

        public IActionResult Save(int userID)
        {
            Request<Pedido> request = new();
            Pedido carrinho = Pedido.ObterCarrinhoDeCompras();

            if (Pedido.UsuarioPossuiPedidosEmAndamento(userID))
            {
               Pedido pedido = Pedido.GetByStatus(userID);
               request.Data = pedido;
            }
            else
            {
                request.Data = carrinho;
                foreach (var itemPedido in carrinho.ItemPedidos)
                {
                    itemPedido.CorID = 0;
                    itemPedido.NomeCor = null;
                    itemPedido.Pedido = null;
                    itemPedido.Produto = null;
                    itemPedido.ID = 0;
                }
            }

            request.UserID = userID;

            Response<Pedido> response = Pedido.Salvar(request);

            //Retorna um objeto JSON com a lista de itens do carrinho
            return Json(response);
        }

        public IActionResult AdicionarItemAoCarrinho(int produtoID, string tamanho, int corID)
        {
            Pedido pedido = Pedido.AdicionarItemAoPedido(produtoID, tamanho, corID);

            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Salva o carrinho de compras no Redis
            db.StringSet("carrinho", System.Text.Json.JsonSerializer.Serialize(pedido), TimeSpan.FromDays(30));

            //Retorna um objeto JSON com a lista de itens do carrinho
            return Json(pedido);
        }

        public IActionResult UpdateByParamentros(decimal valorTotal, decimal valorUnitario, int quantidade, int identificadorItem, string id)
        {
            // Recupera o carrinho de compras do cookie
            Pedido carrinho = Pedido.ObterCarrinhoDeCompras();

            decimal valorAtual = Pedido.CalcularValorItem(valorTotal, valorUnitario, id);
            Pedido pedido = Pedido.AtualizaItemPedido(carrinho, valorAtual, identificadorItem, quantidade);
            Pedido atualizado = Pedido.AtualizaValorCarrinho(pedido);

            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Salva o carrinho de compras no Redis
            db.StringSet("carrinho", System.Text.Json.JsonSerializer.Serialize(atualizado), TimeSpan.FromDays(30));

            //Retorna um objeto JSON com a lista de itens do carrinho
            return Json(pedido);
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
                Pedido.AtualizaValorFrete(Convert.ToDecimal(valor));
                Pedido pedido = Pedido.AdicionarEnderecoAoPedidoAsync(cep);

                var enderecoCompleto = pedido.PedidoEndereco.Bairro + " " + pedido.PedidoEndereco.Logradouro + " - " + pedido.PedidoEndereco.UF;

                var result = new[] { valor, prazoEntrega, enderecoCompleto };
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
