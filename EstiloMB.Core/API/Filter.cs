using System.Collections.Generic;

namespace EstiloMB.Core
{
    public class Filter<T>
    {
        public int UserID { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public T Data { get; set; }
        public List<T> Exclude { get; set; }
    }
}