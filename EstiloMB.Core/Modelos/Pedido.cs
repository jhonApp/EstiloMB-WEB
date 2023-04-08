using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using Sistema;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EstiloMB.Core
{
    [Table("Pedido")]
    public class Pedido
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime DataPedido { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataEntrega { get; set; }
        public decimal Frete { get; set; }
        public int QuantidadeTotal { get; set; }
        public StatusPedido StatusPedido { get; set; }
        public decimal ValorTotalPedido { get; set; }
        public decimal ValorTotalProdutos { get; set; }
        public int? UsuarioID { get; set; }
        public virtual List<ItemPedido> ItemPedidos { get; set; }
        [NotMapped]public virtual PedidoEndereco PedidoEndereco { get; set; }

        [ForeignKey("UsuarioID")]
        public Usuario Usuario { get; set; }

        public static Response<List<Pedido>> Listar(Request<Pedido> request)
        {
            Response<List<Pedido>> response = new Response<List<Pedido>>();

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> usuario = Usuario.Carregar(request.UserID);
                if (usuario.Code != ResponseCode.Sucess)
                {
                    response.Code = usuario.Code;
                    response.Message = usuario.Message;
                    return response;
                }

                using (Database<Pedido> database = new Database<Pedido>())
                {
                    IQueryable<Pedido> query = database.Set<Pedido>()
                                                       .AsNoTracking()
                                                       .OrderByDescending(e => e.ID);

                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    response.Data = query.ToList();
                    response.Code = ResponseCode.Sucess;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static dynamic ObterEndereco(string cep)
        {
            if (!string.IsNullOrEmpty(cep))
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        var response = httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/").Result;
                        response.EnsureSuccessStatusCode();
                        var content = response.Content.ReadAsStringAsync().Result;
                        dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                        return item;
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Erro ao obter endereço: {e.Message}");
                }
            }
            return null;
        }

        public static Pedido AdicionarItemAoPedido(int produtoID, string tamanho, int corID)
        {
            // Recupera o carrinho de compras do cookie
            Pedido carrinho = ObterCarrinhoDeCompras();
            

            // Recupera o produto e a imagem da cor do produto
            var produto = Produto.GetByID(produtoID);
            var produtoImagem = ProdutoImagem.GetByCorID(corID);

            // Verifica se o produto já está no carrinho
            var itemPedidoExistente = carrinho.ItemPedidos.FirstOrDefault(item => item.ProdutoID == produtoID && item.Tamanho == tamanho && item.CorID == corID);
            if (itemPedidoExistente != null)
            {
                // Se o produto já está no carrinho, aumenta a quantidade em 1 e atualiza o valor total do item
                itemPedidoExistente.Quantidade += 1;
                itemPedidoExistente.ValorTotal = produto.Valor * itemPedidoExistente.Quantidade;
            }
            else
            {
                // Se o produto ainda não está no carrinho, adiciona um novo item
                var itemPedido = new ItemPedido
                {
                    ID = GerarIDUnicoAleatorio(),
                    ProdutoID = produtoID,
                    Produto = produto,
                    ImageURL = produtoImagem.ImageURL,
                    Tamanho = tamanho,
                    NomeCor = produtoImagem.Cor.Nome,
                    CorID = corID,
                    Quantidade = 1,
                    ValorTotal = produto.Valor
                };
                carrinho.ItemPedidos.Add(itemPedido);
            }

            // Atualiza os valores totais do carrinho
            carrinho.ValorTotalPedido = carrinho.ItemPedidos.Sum(item => item.ValorTotal);
            carrinho.QuantidadeTotal = carrinho.ItemPedidos.Sum(item => item.Quantidade);

            // Retorna um objeto JSON com a lista de itens do carrinho
            return carrinho;
        }

        public static Pedido AdicionarEnderecoAoPedidoAsync(string cep)
        {
            // Recupera o carrinho de compras do cookie
            Pedido carrinho = ObterCarrinhoDeCompras();

            var endereco = ObterEndereco(cep);

            // Verifica se o pedido já está no carrinho
            if (carrinho.PedidoEndereco != null)
            {
                carrinho.PedidoEndereco.CEP = endereco.cep;
                carrinho.PedidoEndereco.Bairro = endereco.bairro;
                carrinho.PedidoEndereco.Logradouro = endereco.localidade;
                carrinho.PedidoEndereco.UF = endereco.uf;
            }
            else
            {
                // Se o produto ainda não está no carrinho, adiciona um novo item
                var pedidoEndereco = new PedidoEndereco
                {
                    ID = GerarIDUnicoAleatorio(),
                    CEP = endereco.cep,
                    Bairro = endereco.bairro,
                    Logradouro = endereco.localidade,
                    UF = endereco.uf,
                };
                carrinho.PedidoEndereco = pedidoEndereco;
            }

            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Salva o carrinho de compras no Redis
            db.StringSet("carrinho", JsonSerializer.Serialize(carrinho), TimeSpan.FromDays(30));

            // Retorna um objeto JSON com a lista de itens do carrinho
            return carrinho;
        }

        public static Pedido AtualizaItemPedido(Pedido pedido, decimal valorTotalAtual, int id, int quantidade)
        {
            foreach (var itemPedido in pedido.ItemPedidos)
            {
                if (itemPedido.ID == id)
                {
                    itemPedido.Quantidade = quantidade;
                    itemPedido.ValorTotal = valorTotalAtual;
                }
            }

            return pedido;
        }

        public static Pedido AtualizaValorCarrinho(Pedido pedido)
        {
            // Atualiza os valores totais do carrinho
            pedido.ValorTotalPedido = pedido.ItemPedidos.Sum(e => e.ValorTotal) + pedido.Frete;
            pedido.ValorTotalProdutos = pedido.ItemPedidos.Sum(e => e.ValorTotal);

            return pedido;
        }

        public static Pedido AtualizaValorFrete(decimal frete)
        {
            // Recupera o carrinho de compras do cookie
            Pedido carrinho = ObterCarrinhoDeCompras();

            // Atualiza os valores totais do carrinho
            carrinho.Frete = frete;

            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Salva o carrinho de compras no Redis
            db.StringSet("carrinho", JsonSerializer.Serialize(carrinho), TimeSpan.FromDays(30));

            return carrinho;
        }

        public static int GerarIDUnicoAleatorio()
        {
            Random random = new Random();
            Guid id = Guid.NewGuid();

            string uniqueId = id.ToString().Substring(0, 8) +
                              random.Next(100000, 999999).ToString() +
                              random.Next(1000, 9999).ToString() +
                              random.Next(1000, 9999).ToString() +
                              random.Next(100000000, 999999999).ToString();

            int uniqueIntId = uniqueId.GetHashCode();

            return uniqueIntId;
        }

        public static Pedido ObterCarrinhoDeCompras()
        {
            Pedido pedido = new();

            ConnectionMultiplexer redis = RedisConnectionPool.GetConnection();
            IDatabase db = redis.GetDatabase();

            // Obtém o carrinho de compras do Redis
            var carrinhoRedis = db.StringGet("carrinho");

            if (!carrinhoRedis.IsNullOrEmpty)
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
                    pedido = JsonSerializer.Deserialize<Pedido>(carrinhoRedis, options);
                }
                catch (JsonException)
                {
                    // Handle deserialization error here
                }
            }

            if(carrinhoRedis.IsNull == true)
            {
                pedido.ItemPedidos = new List<ItemPedido>();
            }
            

            return pedido;
        }

        public static decimal CalcularValorItem(decimal valorTotalPedido, decimal valorUnitarioItem, string decrementarValor)
        {
            if (valorTotalPedido <= 0 || valorUnitarioItem <= 0)
            {
                throw new ArgumentException("Valor total do pedido e valor unitário do item devem ser maiores que zero.");
            }

            decimal valorItem;

            if (decrementarValor == "decrement")
            {
                valorItem = valorTotalPedido - valorUnitarioItem;
            }
            else
            {
                valorItem = valorTotalPedido + valorUnitarioItem;
            }

            return valorItem;
        }

        public static Response<Pedido> Salvar(Request<Pedido> request)
        {
            Response<Pedido> response = new Response<Pedido>() { Data = request.Data };

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> usuario = Usuario.Carregar(request.UserID);
                if (usuario.Code != ResponseCode.Sucess)
                {
                    response.Code = usuario.Code;
                    response.Message = usuario.Message;
                    return response;
                }

                using (Database<Pedido> database = new Database<Pedido>())
                {

                    // - Validação.
                    //response.Validation = Validation.Validate(request.Data);
                    //// - Erro de validação.
                    //if (!response.Validation.IsValid)
                    //{
                    //    response.Code = ResponseCode.Invalid;
                    //    response.Message = JHIException.ErroValidacao;
                    //    return response;
                    //}

                    request.Data.DataPedido = DateTime.Now;
                    request.Data.StatusPedido = StatusPedido.EmAndamento;
                    request.Data.UsuarioID = usuario.Data.UsuarioID;

                    if (request.Data.ID == 0)
                    {
                        database.Set<Pedido>().Add(request.Data);
                        database.SaveChanges();
                    }
                    else
                    {
                        database.Set<Pedido>().Update(request.Data);
                        database.SaveChanges();
                    }
                }

                response.Code = ResponseCode.Sucess;
                response.Message = JHIException.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Pedido,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static bool IsValidCreditCardNumber(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "").Replace("-", ""); // Remove espaços e hífens
            if (!cardNumber.All(char.IsDigit)) // Verifica se o número contém apenas dígitos
            {
                return false;
            }

            int sum = 0;
            bool alternate = false;
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0; // Retorna verdadeiro se a soma for um múltiplo de 10
        }

        public static string GetCardBrand(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "").Replace("-", ""); // Remove espaços e hífens
            string bin = cardNumber.Substring(0, 6); // Obtém os primeiros seis dígitos

            using (HttpClient client = new HttpClient())
            {
                string url = "https://lookup.binlist.net/" + bin;
                HttpResponseMessage response = client.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    return data.brand;
                }
                else
                {
                    return null;
                }
            }
        }

        public static Pedido GetByID(int pedidoID)
        {
            try
            {
                using Database<Pedido> db = new Database<Pedido>();
                return db.Set<Pedido>()
                    .Where(p => p.ID == pedidoID)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Pedido GetByUsuarioID(int usuarioID)
        {
            Pedido pedido = new Pedido();
            try
            {
                using Database<Pedido> db = new Database<Pedido>();
                return db.Set<Pedido>()
                    .Where(p => p.UsuarioID == usuarioID)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Pedido GetByStatus(int usuarioID)
        {
            Pedido pedido = new Pedido();
            try
            {
                using Database<Pedido> db = new Database<Pedido>();
                return db.Set<Pedido>()
                    .Include(p => p.Usuario)
                    .Where(p => p.UsuarioID == usuarioID && p.StatusPedido == StatusPedido.EmAndamento)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool UsuarioPossuiPedidosEmAndamento(int usuarioID)
        {
            using Database<Pedido> db = new Database<Pedido>();
            return db.Set<Pedido>()
                     .Any(p => p.UsuarioID == usuarioID && p.StatusPedido == StatusPedido.EmAndamento);
        }

        public static Pedido CreatePedido()
        {
            Pedido pedido = new Pedido();

            try
            {
                using Database<Pedido> database = new Database<Pedido>();
                
                pedido.DataPedido = DateTime.Now;
                pedido.StatusPedido = StatusPedido.EmAndamento;

                database.Set<Pedido>().Add(pedido);
                database.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pedido;
        }

        public static Response<Pedido> Remover(Request<Pedido> request)
        {
            Response<Pedido> response = new Response<Pedido>() { Data = request.Data };

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> usuario = Usuario.Carregar(request.UserID);
                if (usuario.Code != ResponseCode.Sucess)
                {
                    response.Code = usuario.Code;
                    response.Message = usuario.Message;
                    return response;
                }

                using (Database<Pedido> database = new Database<Pedido>())
                {
                    // - Modelo original.
                    Pedido original = response.Data.ID != 0 ? database.Set<Pedido>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<Pedido>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Pedido,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.Pedido
                    }
                       .Parameters(usuario.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                }

                response.Code = ResponseCode.Sucess;
                response.Message = JHIException.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Pedido,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static void Remove(int pedidoID)
        {
            try
            {
                using Database<Pedido> db = new Database<Pedido>();
                Pedido pedido = db.Set<Pedido>()
                    .Where(p => p.ID == pedidoID)
                    .FirstOrDefault();

                db.Set<Pedido>().Remove(pedido);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
