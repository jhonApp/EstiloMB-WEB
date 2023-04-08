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
    [Table("Estoque")]
    public class Estoque
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
        public Produto Produto { get; set; }

        public static Response<List<Estoque>> Listar(Request<Estoque> request)
        {
            Response<List<Estoque>> response = new Response<List<Estoque>>();

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

                using (Database<Estoque> database = new Database<Estoque>())
                {
                    IQueryable<Estoque> query = database.Set<Estoque>()
                                                       .AsNoTracking()
                                                       .Include(e => e.Produto)
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

        public static Response<Estoque> Salvar(Request<Estoque> request)
        {
            Response<Estoque> response = new Response<Estoque>() { Data = request.Data };

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

                using (Database<Estoque> database = new Database<Estoque>())
                {

                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo nome.
                    if (database.Set<Estoque>().Count(e => e.Produto.Nome == request.Data.Produto.Nome && e.ID != request.Data.ID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Estoque>(Text.EstoqueNomeJaRegistrado, e => e.Produto.Nome);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = JHIException.ErroValidacao;
                        return response;
                    }

                    // - Modelo original.
                    Estoque original = database.Set<Estoque>()
                                                .AsNoTracking()
                                                .Include(e => e.Produto)
                                                .FirstOrDefault(e => e.ID == request.Data.ID);


                    if (request.Data.ID == 0)
                    {
                        database.Set<Estoque>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Estoque,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.EstoqueCriado
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Produto.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {
                        database.Set<Estoque>().Update(request.Data);
                        database.SaveChanges();

                        request.Data.Produto = original.Produto;

                        new Log()
                        {
                            Entity = Text.Estoque,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.EstoqueCriado
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Produto.Nome)
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
                    Entity = Text.Estoque,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static Response<Estoque> Remover(Request<Estoque> request)
        {
            Response<Estoque> response = new Response<Estoque>() { Data = request.Data };

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

                using (Database<Estoque> database = new Database<Estoque>())
                {
                    // - Modelo original.
                    Estoque original = response.Data.ID != 0 ? database.Set<Estoque>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<Estoque>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Estoque,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.Estoque
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
                    Entity = Text.Estoque,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }
    }
}
