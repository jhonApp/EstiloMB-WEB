using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Produto.Tamanho")]
    public class ProdutoTamanho
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public int TamanhoID { get; set; }
        public decimal Peso { get; set; }
        public decimal Comprimento { get; set; }
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public StatusParametro Status { get; set; }
        public Tamanho Tamanho { get; set; }
    }
}
