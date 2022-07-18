using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chargeback.Core
{
    [Table("Log.Parameter")]
    public class Parameter
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ParameterID { get; set; }

        public int LogID { get; set; }
        public string Value { get; set; }
    }
}