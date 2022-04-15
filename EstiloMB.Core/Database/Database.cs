using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
using System;

namespace EstiloMB.Core
{
    public abstract class Database : DbContext
    {
        private static SQLVersion _version = SQLVersion.SQL2016;
        private static string _defaultConnectionString = null;
        private string _connectionString;
        //private static readonly ILoggerFactory loggerFactory = new ServiceCollection().AddLogging(builder => builder.AddConsole().AddFilter(DbLoggerCategory.Database.Command.Name, LogLevel.Information))
        //                                                                              .BuildServiceProvider()
        //                                                                              .GetService<ILoggerFactory>();

        protected Database()
        {
            _connectionString = _defaultConnectionString;
        }

        protected Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //optionsBuilder.UseLoggerFactory(loggerFactory);

                switch (_version)
                {
                    case SQLVersion.SQL2012:
                        {
                            optionsBuilder.UseSqlServer(_connectionString, o => {
                                //o.UseRowNumberForPaging();
                                o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                            });
                            break;
                        }
                    case SQLVersion.SQL2016:
                    default:
                        {
                            optionsBuilder.UseSqlServer(_connectionString, o => {
                                o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                            });
                            break;
                        }
                }
            }
        }

        public static void Configure(string connectionString, SQLVersion version = SQLVersion.SQL2016)
        {
            _defaultConnectionString = connectionString;
            _version = version;
        }

        public enum SQLVersion
        {
            SQL2016 = 0,
            SQL2012 = 1
        }
    }

    public class Database<T> : Database where T : class
    {
        public Database() : base()
        { }

        public Database(string connectionString) : base(connectionString)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>();
        }
    }

    public class Database<T, T1> : Database where T : class
                                            where T1 : class
    {
        public Database() : base()
        { }

        public Database(string connectionString) : base(connectionString)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>();
            modelBuilder.Entity<T1>();
        }
    }

    public class Database<T, T1, T2> : Database where T : class
                                                where T1 : class
                                                where T2 : class
    {
        public Database() : base()
        { }

        public Database(string connectionString) : base(connectionString)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>();
            modelBuilder.Entity<T1>();
            modelBuilder.Entity<T2>();
        }
    }

    public class Database<T, T1, T2, T3> : Database where T : class
                                                    where T1 : class
                                                    where T2 : class
                                                    where T3 : class
    {
        public Database() : base()
        { }

        public Database(string connectionString) : base(connectionString)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<T>();
            modelBuilder.Entity<T1>();
            modelBuilder.Entity<T2>();
            modelBuilder.Entity<T3>();
        }
    }
}