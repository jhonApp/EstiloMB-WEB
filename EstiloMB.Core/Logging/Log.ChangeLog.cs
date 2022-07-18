namespace Chargeback.Core
{
    public class ChangeLog
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public object NewValue { get; set; }
        public ChangeLogType Type { get; set; }
        public ChangeLogOperation Operation { get; set; }
    }

    public enum ChangeLogType
    {
        String = 0,
        Date = 1,
        Object = 2,
        List = 3
    }

    public enum ChangeLogOperation
    {
        NotAvailable = 0,
        Unmodified = 1,
        Insert = 2,
        Update = 3,
        Delete = 4
    }
}