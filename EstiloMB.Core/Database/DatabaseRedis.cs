using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstiloMB.Core
{
    public static class RedisConnectionPool
    {
        private static ConnectionMultiplexer _connection;

        public static ConnectionMultiplexer GetConnection()
        {
            if (_connection == null)
            {
                _connection = ConnectionMultiplexer.Connect("localhost:6379");
            }

            return _connection;
        }
    }
}
