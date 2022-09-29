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
    [Table("Produto")]
    public class Produto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(255), LogProperty]
        public string Nome { get; set; }

        [Required, MaxLength(255), LogProperty]
        public string Descricao { get; set; }
        public decimal Valor { get; set; }

        public List<ProdutoImagem> ProdutoImagens { get; set; }
        public List<ProdutoCategoria> ProdutoCategorias { get; set; }
        public List<ProdutoTamanho> ProdutoTamanhos { get; set; }
        public List<ProdutoCor> ProdutoCores { get; set; }

        public static Response<List<Produto>> Listar(Request<Produto> request)
        {
            Response<List<Produto>> response = new Response<List<Produto>>();

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

                using (Database<Produto> database = new Database<Produto>())
                {
                    IQueryable<Produto> query = database.Set<Produto>()
                                                       .AsNoTracking()
                                                       .OrderBy(e => e.ID);

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

        public static Response<Produto> Salvar(Request<Produto> request)
        {
            Response<Produto> response = new Response<Produto>() { Data = request.Data };

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

                using (Database<Produto> database = new Database<Produto>())
                {
                    // - Validação.
                    response.Validation = Validation.Validate(request.Data);

                    // - Validação de mesmo nome.
                    if (database.Set<Produto>().Count(e => e.Nome == request.Data.Nome && e.ID != request.Data.ID) > 0)
                    {
                        response.Validation.IsValid = false;
                        response.Validation.Add<Produto>(Text.ProdutoNomeJaRegistrado, e => e.Nome);
                    }

                    // - Erro de validação.
                    if (!response.Validation.IsValid)
                    {
                        response.Code = ResponseCode.Invalid;
                        response.Message = JHIException.ErroValidacao;
                        return response;
                    }

                    // - Modelo original.
                    Produto original = response.Data.ID != 0 ? database.Set<Produto>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;

                    if (request.Data.ID == 0)
                    {
                        database.Set<Produto>().Add(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Produto,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.ProdutoCriado
                        }
                       .Parameters(usuario.Data.Nome, request.Data.Nome)
                       .Data(null, request.Data)
                       .Save();
                    }
                    else
                    {
                        database.Set<Produto>().Update(request.Data);
                        database.SaveChanges();

                        new Log()
                        {
                            Entity = Text.Produto,
                            EntityID = request.Data.ID,
                            LogType = LogTipo.Historico,
                            UserID = request.UserID,
                            Message = Text.ProdutoCriado
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
