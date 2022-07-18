using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chargeback.Core
{
    [Table("Log.Data")]
    public class LogData
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DataID { get; set; }

        public int LogID { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public DataType DataType { get; set; }
    }
}