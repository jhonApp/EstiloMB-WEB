using Chargeback.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstiloMB.Core
{
    [Table("Pedido.Endereco")]
    public class PedidoEndereco
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int PaisID { get; set; }
        public int EstadoID { get; set; }
        public int CidadeID { get; set; }
        public int UsuarioID { get; set; }

        [MaxLength(50), LogProperty]
        public string Logradouro { get; set; }

        [MaxLength(6), LogProperty]
        public string Numero { get; set; }

        [MaxLength(50), LogProperty]
        [NotMapped]public string UF { get; set; }

        [MaxLength(50), LogProperty]
        public string Complemento { get; set; }

        [MaxLength(30), LogProperty]
        public string Bairro { get; set; }

        [MaxLength(9), LogProperty]
        public string CEP { get; set; }
    }
}
