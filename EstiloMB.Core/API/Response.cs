using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace EstiloMB.Core
{
    public class Response
    {
        public int Total { get; set; }
        public ResponseCode Code { get; set; }
        public string Message { get; set; }
        public Validation Validation { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public class Response<T> : Response
    {
        public T Data { get; set; }
    }

    public enum ResponseCode
    {
        Sucess = 0,

        Denied = 10,
        NotFound = 11,
        WrongCredentials = 12,
        Expired = 13,

        Invalid = 97,
        BadRequest = 98,
        ServerError = 99
    }
}
