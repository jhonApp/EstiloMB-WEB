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
    [Table("Cor")]
    public class Cor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }
        
        [Required, MaxLength(50), LogProperty]
        public string Hexadecimal { get; set; }
        public static Response<List<Cor>> Listar(Request<Cor> request)
        {
            Response<List<Cor>> response = new Response<List<Cor>>();

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

                using (Database<Cor> database = new Database<Cor>())
                {
                    IQueryable<Cor> query = database.Set<Cor>()
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
                    Entity = Text.Cor,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Cor> Salvar(Request<Cor> request)
        {
            Response<Cor> response = new Response<Cor>() { Data = request.Data };

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

                using (Database<Cor> database = new Database<Cor>())
                {

                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo nome.
                    if (database.Set<Cor>().Count(e => e.Nome == request.Data.Nome && e.ID != request.Data.ID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Cor>(Text.CorNomeJaRegistrado, e => e.Nome);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = JHIException.ErroValidacao;
                        return response;
                    }

                    // - Modelo original.
                    Cor original = database.Set<Cor>()
                                                .AsNoTracking()
                                                .FirstOrDefault(e => e.ID == request.Data.ID);


                    if (request.Data.ID == 0)
                    {
                        database.Set<Cor>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Cor,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.CorCriada
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {

                        database.Set<Cor>().Update(request.Data);
                        database.SaveChanges();
                        new Log()
                        {
                            Entity = Text.Cor,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.CorCriada
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
                    Entity = Text.Cor,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
               .Data(request.Data)
               .Save();
            }

            return response;
        }

        public static Response<Cor> Remover(Request<Cor> request)
        {
            Response<Cor> response = new Response<Cor>() { Data = request.Data };

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



                using (Database<Cor, ProdutoCor> database = new Database<Cor, ProdutoCor>())
                {
                    // - Modelo original.
                    Cor original = response.Data.ID != 0 ? database.Set<Cor>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    // - Validação de mesmo nome.
                    if (database.Set<ProdutoCor>().Count(e => e.CorID == request.Data.ID) > 0)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.CorEmUso;
                        return response;
                    }

                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<Cor>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Cor,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.Cor
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
