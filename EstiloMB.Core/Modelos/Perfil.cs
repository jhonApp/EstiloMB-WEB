using EstiloMB.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Chargeback.Core
{
    [Table("Perfil")]
    public class Perfil
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }
        public int Status { get; set; }

        [LogProperty]
        public List<Acao> Acoes { get; set; }

        public static Response<List<Perfil>> Listar(Request<Perfil> request)
        {
            Response<List<Perfil>> response = new Response<List<Perfil>>();

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> user = Usuario.Carregar(request.UserID);
                if (user.Code != ResponseCode.Sucess)
                {
                    response.Code = user.Code;
                    response.Message = user.Message;
                    return response;
                }

                // - Verificando permissão.
                if (!user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Perfil> database = new Database<Perfil>())
                {
                    IQueryable<Perfil> query = database.Set<Perfil>()
                                                     .AsNoTracking()
                                                     .Include(e => e.Acoes)
                                                     .OrderBy(e => e.Nome);
                    // - Aplicando filtros.
                    //for (int i = 0; i < criterias.Length; i++)
                    //{
                    //    query = query.Where(criterias[i]);
                    //}

                    // - Paginação.
                    response.Total = query.Count();
                    if (request.Page > 0) { query = query.Skip(request.PerPage * (request.Page - 1)); }
                    if (request.PerPage > 0) { query = query.Take(request.PerPage); }

                    // - Resultado.
                    response.Data = query.ToList();
                    response.Code = ResponseCode.Sucess;
                    response.Message = Text.Sucesso;
                }
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Perfis,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Perfil> Salvar(Request<Perfil> request)
        {
            Response<Perfil> response = new Response<Perfil>() { Data = request.Data };

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> user = Usuario.Carregar(request.UserID);
                if (user.Code != ResponseCode.Sucess)
                {
                    response.Code = user.Code;
                    response.Message = user.Message;
                    return response;
                }

                using (Database<Perfil> database = new Database<Perfil>())
                {
                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo nome.
                    if (database.Set<Perfil>().Count(e => e.Nome == request.Data.Nome && e.ID != request.Data.ID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Perfil>(Text.PerfilNomeJaRegistrado, e => e.Nome);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = Text.ErroValidacao;
                        return response;
                    }

                    // - Obtendo o original.
                    Perfil original = response.Data.ID != 0 ? database.Set<Perfil>()
                                                                            .AsNoTracking()
                                                                            .Include(e => e.Acoes)
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    if (request.Data.ID == 0)
                    {
                        database.Set<Perfil>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Perfis,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.PerfilAdicionado
                        }
                        .Parameters(user.Data.Nome, request.Data.Nome)
                        .Data(null, request.Data)
                        .Save();
                    }
                    else
                    {
                        database.Set<Acao>().RemoveRange(original.Acoes.Where(e => !request.Data.Acoes.Select(a => a.Nome).Contains(e.Nome)));
                        for (int i = 0; i < request.Data.Acoes.Count; i++)
                        {
                            request.Data.Acoes[i].AcaoID = original.Acoes.FirstOrDefault(e => e.Nome == request.Data.Acoes[i].Nome)?.AcaoID ?? 0;
                        }

                        database.Set<Perfil>().Update(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Perfis,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.PerfilAtualizado
                        }
                        .Parameters(user.Data.Nome, request.Data.Nome)
                        .Data(original, request.Data)
                        .Save();
                    }
                }

                response.Code = ResponseCode.Sucess;
                response.Message = Text.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Perfis,
                    EntityID = request.Data.ID,
                    LogType = LogTipo.Exception,
                    UserID = request.UserID,
                    Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "")
                }
                .Data(request.Data)
                .Save();
            }

            return response;
        }

        public static Response<Perfil> Remover(Request<Perfil> request)
        {
            Response<Perfil> response = new Response<Perfil>() { Data = request.Data };

            try
            {
                // - Usuário que chamou esta ação.
                Response<Usuario> user = Usuario.Carregar(request.UserID);
                if (user.Code != ResponseCode.Sucess)
                {
                    response.Code = user.Code;
                    response.Message = user.Message;
                    return response;
                }

                // - Verificando permissão.
                if (!user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Perfil> database = new Database<Perfil>())
                {
                    // - Obtendo o original.
                    Perfil original = database.Set<Perfil>().FirstOrDefault(e => e.ID == request.Data.ID);
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.ErroRequisicao;
                        return response;
                    }

                    database.Set<Perfil>().Update(original);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Perfis,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.PerfilRemovido
                    }
                    .Data(user.Data.Nome, request.Data.Nome)
                    .Data(original)
                    .Save();
                }

                response.Code = ResponseCode.Sucess;
                response.Message = Text.Sucesso;
            }
            catch (Exception ex)
            {
                response.Code = ResponseCode.ServerError;
                response.Message = ex.Message + (ex.InnerException != null ? ", " + ex.InnerException.Message : "");

                new Log()
                {
                    Entity = Text.Perfis,
                    EntityID = request.Data.ID,
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