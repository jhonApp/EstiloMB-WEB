using Chargeback.Core;
using Microsoft.EntityFrameworkCore;
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
        public int CategoriaID { get; set; }

        [Required, MaxLength(255), LogProperty]
        public string Nome { get; set; }

        [Required, MaxLength(255), LogProperty]
        public string Descricao { get; set; }
        public int Tamanho { get; set; }
        public decimal Valor { get; set; }
        public int Cor { get; set; }
        public string ImageURL { get; set; }
        [NotMapped] public byte[] ImageData { get; set; }

        [ForeignKey("CategoriaID")]
        public Categoria Categoria { get; set; }

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
                                                       .Include(e => e.Categoria)
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
    }
}
