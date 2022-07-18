using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chargeback.Core
{
    [Table("Usuario.Login")]
    public class Login
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginID { get; set; }
        public int UsuarioID { get; set; }
        public string SessionID { get; set; }
        public string IpAddress { get; set; }
        public DateTime DataRegistro { get; set; }
    }
}