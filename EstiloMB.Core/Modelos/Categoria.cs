using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
using Sistema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Categoria")]
    public class Categoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }

        public static Response<List<Categoria>> Listar(Request<Categoria> request)
        {
            Response<List<Categoria>> response = new Response<List<Categoria>>();

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

                // - Verificando permissão.
                //if (!usuario.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Componentes && e.Habilitado == true)))
                //{
                //    response.Code = ResponseCode.Denied;
                //    response.Message = Text.AcessoNegado;
                //    return response;
                //}

                using (Database<Categoria> database = new Database<Categoria>())
                {
                    IQueryable<Categoria> query = database.Set<Categoria>()
                                                       .AsNoTracking()
                                                       .OrderBy(e => e.Nome);

                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    response.Data = query.ToList();
                    response.Code = ResponseCode.Sucess;
                    response.Message = JHIException.Sucesso;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Categoria,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Categoria> Salvar(Request<Categoria> request)
        {
            Response<Categoria> response = new Response<Categoria>() { Data = request.Data };

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

                using (Database<Categoria> database = new Database<Categoria>())
                {

                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo nome.
                    if (database.Set<Categoria>().Count(e => e.Nome == request.Data.Nome && e.ID != request.Data.ID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Categoria>(Text.CategoriaNomeJaRegistrado, e => e.Nome);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = JHIException.ErroValidacao;
                        return response;
                    }

                    // - Modelo original.
                    Categoria original = database.Set<Categoria>()
                                                .AsNoTracking()
                                                .FirstOrDefault(e => e.ID == request.Data.ID);


                    if (request.Data.ID == 0)
                    {
                        database.Set<Categoria>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Categoria,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.CategoriaCriada
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {

                        database.Set<Categoria>().Update(request.Data);
                        database.SaveChanges();
                        new Log()
                        {
                            Entity = Text.Categoria,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.CategoriaCriada
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Nome)
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
                    Entity = Text.Categoria,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static Response<Categoria> Remover(Request<Categoria> request)
        {
            Response<Categoria> response = new Response<Categoria>() { Data = request.Data };

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

                

                using (Database<Categoria, ProdutoCategoria> database = new Database<Categoria, ProdutoCategoria>())
                {
                    // - Modelo original.
                    Categoria original = response.Data.ID != 0 ? database.Set<Categoria>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    // - Validação de mesmo nome.
                    if (database.Set<ProdutoCategoria>().Count(e => e.CategoriaID == request.Data.ID) > 0)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.CategoriaEmUso;
                        return response;
                    }

                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<Categoria>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Categoria,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.Categoria
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
                    Entity = Text.Produto,
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
