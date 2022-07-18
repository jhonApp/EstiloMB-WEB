using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chargeback.Core
{
    [Table("Perfil.Acao")]
    public class Acao
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcaoID { get; set; }
        public int PerfilID { get; set; }

        [Required, MaxLength(50), LogProperty]
        public string Nome { get; set; }

        [MaxLength(50), LogProperty]
        public string Grupo { get; set; }

        [MaxLength(100), LogProperty]
        public string Valor { get; set; }

        [LogProperty]
        public bool Habilitado { get; set; }
    }
}