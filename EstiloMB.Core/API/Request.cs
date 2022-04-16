using System;
using System.Collections.Generic;

namespace EstiloMB.Core
{
    public class Request
    {
        public int UserID { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public string Culture { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class Request<T> : Request
    {
        public T Data { get; set; }
        public List<T> Filter { get; set; }
    }
}