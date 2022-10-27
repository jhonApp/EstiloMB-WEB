using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Produto.Categoria")]
    public class ProdutoCategoria
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int ProdutoID { get; set; }
        public int CategoriaID { get; set; }
        public Categoria Categoria { get; set; }

    }
}
