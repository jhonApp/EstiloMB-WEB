using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chargeback.Core
{
    [Table("UsuarioPerfil")]
    public class UsuarioPerfil
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UsuarioPerfilID { get; set; }
        public int UsuarioID { get; set; }
        public int PerfilID { get; set; }
        public Perfil Perfil { get; set; }
    }
}