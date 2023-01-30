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
        public StatusParametro Status { get; set; }
        public List<ProdutoCategoria> ProdutoCategorias { get; set; }
        public List<ProdutoTamanho> ProdutoTamanhos { get; set; }
        public List<ProdutoImagem> ProdutoImagens { get; set; }

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
                                                       .Include(e => e.ProdutoImagens)
                                                       .Include(e => e.ProdutoCategorias).ThenInclude(e => e.Categoria)
                                                       .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
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

        public static Produto GetByID(int produtoID)
        {
            try
            {
                using Database<Produto> db = new Database<Produto>();
                return db.Set<Produto>()
                    .Include(e => e.ProdutoCategorias)
                    .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
                    .Include(e => e.ProdutoImagens).ThenInclude(e => e.Cor)
                    .Include(e => e.ProdutoImagens)
                    .Where(p => p.ID == produtoID)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Response<List<Produto>> Parametro(Request<Produto> request)
        {
            Response<Produto> response = new Response<Produto>() { Data = request.Data };
            Response<List<Produto>> listResponse = new Response<List<Produto>>();

            response = Salvar(request);

            if (response.Code == ResponseCode.Sucess)
            {
                listResponse = Listar(request);
            }

            return listResponse;
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
                    Produto original = database.Set<Produto>()
                                                .AsNoTracking()
                                                .Include(e => e.ProdutoCategorias).ThenInclude(e => e.Categoria)
                                                .Include(e => e.ProdutoImagens)
                                                .Include(e => e.ProdutoTamanhos).ThenInclude(e => e.Tamanho)
                                                .FirstOrDefault(e => e.ID == request.Data.ID);


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


                        List<ProdutoImagem> Imagems = original.ProdutoImagens.Where(e => !request.Data.ProdutoImagens.Select(r => r.ID).Contains(e.ID)).ToList();

                        if (Imagems != null)
                        {
                            foreach (var item in Imagems)
                            {
                                ProdutoImagem.Remove(item.ID);
                            }
                        }

                        for (int i = 0; i < request.Data.ProdutoImagens.Count; i++)
                        {
                            request.Data.ProdutoImagens[i].ProdutoID = original.ProdutoImagens.FirstOrDefault(e => e.ProdutoID == request.Data.ProdutoImagens[i].ProdutoID)?.ProdutoID ?? 0;
                        }

                        database.Set<Produto>().Update(request.Data);
                        database.SaveChanges();

                        request.Data.ProdutoCategorias = original.ProdutoCategorias;
                        request.Data.ProdutoTamanhos = original.ProdutoTamanhos;

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

                    string directory = Environment.CurrentDirectory + "\\wwwroot\\Produtos";

                    //currentDirectory = directory + "Perfil\\";
                    if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

                    for (int i = 0; i < request.Data.ProdutoImagens?.Count; i++)
                    {
                        ProdutoImagem originalFile = request.Data.ProdutoImagens[i].ID > 0 ? original?.ProdutoImagens.FirstOrDefault(e => e.ID == request.Data.ProdutoImagens[i].ID) : null;

                        // - Imagem vazia ou nova imagem, apagando a anterior se existir.
                        if ((request.Data.ProdutoImagens[i].ImageURL == null || request.Data.ProdutoImagens[i].ImageData != null) && originalFile != null)
                        {
                            File.Delete(directory + originalFile.ImageURL);
                        }

                        // - Salvando o novo arquivo recebido
                        if (request.Data.ProdutoImagens[i].ImageData != null && request.Data.ProdutoImagens[i].ImageData.Length > 0)
                        {
                            try
                            {
                                File.WriteAllBytes(directory + "\\" + request.Data.ProdutoImagens[i].ImageURL, request.Data.ProdutoImagens[i].ImageData);
                            }
                            catch //(Exception ex)
                            {
                                database.Set<ProdutoImagem>().Remove(request.Data.ProdutoImagens[i]);
                            }
                        }
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

        public static Response<Produto> Remover(Request<Produto> request)
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
                    // - Modelo original.
                    Produto original = response.Data.ID != 0 ? database.Set<Produto>()
                                                                            .AsNoTracking()
                                                                            .FirstOrDefault(e => e.ID == request.Data.ID)
                                                                            : null;
                    if (original == null)
                    {
                        response.Code = ResponseCode.BadRequest;
                        response.Message = JHIException.ErroRequisicao;
                        return response;
                    }

                    database.Set<Produto>().Remove(request.Data);
                    database.SaveChanges();

                    new Log()
                    {
                        Entity = Text.Produto,
                        EntityID = request.Data.ID,
                        LogType = LogTipo.Historico,
                        UserID = request.UserID,
                        Message = Text.Produto
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
