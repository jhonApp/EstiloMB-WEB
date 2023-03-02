using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using Sistema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;

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
        public decimal ValorTotal { get; set; }
        public int? UsuarioID { get; set; }
        [NotMapped] public List<ItemPedido> itemPedidos { get; set; }

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

        //public static Pedido GetPedidoByUsuario(int usuarioID)
        //{
        //    try
        //    {
        //        using Database<Pedido> db = new Database<Pedido>();
        //        return db.Set<Pedido>()
        //            .Include(e => e.ProdutoCategorias)
        //            .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
        //            .Include(e => e.ProdutoImagens).ThenInclude(e => e.Cor)
        //            .Include(e => e.ProdutoImagens)
        //            .Where(p => p.ID == produtoID)
        //            .FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

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

                        new Log()
                        {
                            Entity = Text.Pedido,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.PedidoCriado
                        }
                       .Parameters(usuario.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {
                        database.Set<Pedido>().Update(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Pedido,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.PedidoCriado
                        }
                       .Parameters(usuario.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
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
