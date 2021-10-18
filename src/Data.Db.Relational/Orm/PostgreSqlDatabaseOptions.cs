using Dapper.FastCrud.Configuration;

namespace RecShark.Data.Db.Relational.Orm
{
    public class PostgreSqlDatabaseOptions : SqlDatabaseOptions
    {
        public PostgreSqlDatabaseOptions()
        {
            StartDelimiter = EndDelimiter = string.Empty;
            IsUsingSchemas = true;
        }
    }
}