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
        public int PerfilID { get; set; }
        public int EmpresaID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }

        [MaxLength(100), LogProperty]
        public string Descricao { get; set; }

        [LogProperty]
        public List<Acao> Acoes { get; set; }
        public DateTime RegistradoEm { get; set; }
        public int? RegistradoPor { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public int? AtualizadoPor { get; set; }

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
                if (!user.Data.Kstack && !user.Data.Admin && !user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
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

                    if (!user.Data.Kstack)
                    {
                        query = query.Where(e => e.EmpresaID == user.Data.EmpresaID);
                    }

                    if (request.Data != null && request.Data.EmpresaID != 0)
                    {
                        query = query.Where(e => e.EmpresaID == request.Data.EmpresaID);
                    }

                    if (request.Filter?.Count > 0)
                    {
                        query = query.Where(e => !request.Filter.Select(r => r.PerfilID).Contains(e.PerfilID));
                    }

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
                    if (database.Set<Perfil>().Count(e => e.Nome == request.Data.Nome && e.PerfilID != request.Data.PerfilID && e.EmpresaID == request.Data.EmpresaID) > 0)
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
                    Perfil original = response.Data.PerfilID != 0 ? database.Set<Perfil>()
                                                                            .AsNoTracking()
                                                                            .Include(e => e.Acoes)
                                                                            .FirstOrDefault(e => e.PerfilID == request.Data.PerfilID)
                                                                            : null;

                    if (!user.Data.Kstack && original != null && original.EmpresaID != user.Data.EmpresaID)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = Text.AcessoNegado;
                        return response;
                    }

                    request.Data.EmpresaID = original?.EmpresaID ?? (!user.Data.Kstack ? user.Data.EmpresaID : request.Data.EmpresaID);
                    if (request.Data.EmpresaID == 0)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.ErroRequisicao;
                        return response;
                    }

                    request.Data.RegistradoPor = original?.RegistradoPor ?? request.UserID;
                    request.Data.RegistradoEm = original?.RegistradoEm ?? DateTime.Now;
                    request.Data.AtualizadoPor = request.UserID;
                    request.Data.AtualizadoEm = DateTime.Now;

                    if (request.Data.PerfilID == 0)
                    {
                        database.Set<Perfil>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Perfis,
                            EntityID = request.Data.PerfilID,
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
                            EntityID = request.Data.PerfilID,
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
                    EntityID = request.Data.PerfilID,
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
                if (!user.Data.Kstack && !user.Data.Admin && !user.Data.Perfis.Any(e => e.Perfil.Acoes.Any(e => e.Nome == Text.Aprovador && e.Habilitado == true)))
                {
                    response.Code = ResponseCode.Denied;
                    response.Message = Text.AcessoNegado;
                    return response;
                }

                using (Database<Perfil> database = new Database<Perfil>())
                {
                    // - Obtendo o original.
                    Perfil original = database.Set<Perfil>().FirstOrDefault(e => e.PerfilID == request.Data.PerfilID);
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = Text.ErroRequisicao;
                        return response;
                    }

                    if (!user.Data.Kstack && original.EmpresaID != user.Data.EmpresaID)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = Text.AcessoNegado;
                        return response;
                    }

                    original.AtualizadoEm = DateTime.Now;
                    original.AtualizadoPor = request.UserID;

                    database.Set<Perfil>().Update(original);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Perfis,
                        EntityID = request.Data.PerfilID,
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
                    EntityID = request.Data.PerfilID,
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