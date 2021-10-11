using System.Data;
using Dapper.FastCrud;

namespace RecShark.Data.Db.Relational
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateDbConnection(string type, string name);

        SqlDialect GetSqlDialect(string type);
    }
}