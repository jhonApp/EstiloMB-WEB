using Chargeback.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    }
}
