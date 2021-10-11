using System;
using System.Data;
using Dapper.FastCrud;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace RecShark.Data.Db.Relational
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConnectionStrings connectionStrings;

        public DbConnectionFactory(IConnectionStrings connectionStrings)
        {
            this.connectionStrings = connectionStrings;
        }

        public IDbConnection CreateDbConnection(string type, string name)
        {
            var connectionString = this.connectionStrings.Get(name);

            switch (type)
            {
                case "postgre": return new NpgsqlConnection(connectionString.Value);
                case "oracle":  return new OracleConnection(connectionString.Value);
                default:
                    throw new NotSupportedException($"Type {type} is not managed yet.");
            }
        }

        public SqlDialect GetSqlDialect(string type)
        {
            switch (type)
            {
                case "postgre": return SqlDialect.PostgreSql;
                case "oracle":  return SqlDialect.MySql;
                default:
                    throw new NotSupportedException($"Type {type} is not managed yet.");
            }
        }
    }
}