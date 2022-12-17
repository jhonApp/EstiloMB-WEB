using Chargeback.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Produto.Imagem")]
    public class ProdutoImagem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public StatusParametro Status { get; set; }
        [Required, MaxLength(255), LogProperty] public string ImageURL { get; set; }
        [NotMapped] public byte[] ImageData { get; set; }

        public static Response<ProdutoImagem> Remove(int ID)
        {
            Response<ProdutoImagem> reg = null;

            try
            {
                using (Database<ProdutoImagem> database = new Database<ProdutoImagem>())
                {
                    ProdutoImagem produtoImagem = database.Set<ProdutoImagem>().Where(e => e.ID == ID).FirstOrDefault();
                    database.Remove(produtoImagem);
                    database.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return reg;
        }
    }
}
