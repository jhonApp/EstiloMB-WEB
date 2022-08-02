using Chargeback.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EstiloMB.Core
{
    [Table("Usuario.Endereco")]
    internal class UsuarioEndereco
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioEnderecoID { get; set; }
        public int PaisID { get; set; }
        public int EstadoID { get; set; }
        public int CidadeID { get; set; }
        public int UsuarioID { get; set; }

        [MaxLength(50), LogProperty]
        public string Logradouro { get; set; }

        [MaxLength(6), LogProperty]
        public string Numero { get; set; }

        [MaxLength(50), LogProperty]
        public string Complemento { get; set; }

        [MaxLength(30), LogProperty]
        public string Bairro { get; set; }

        [MaxLength(9), LogProperty]
        public string CEP { get; set; }
    }
}
