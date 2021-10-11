using Dapper.FastCrud.Configuration;

namespace RecShark.Data.Db.Relational.Orm
{
    public class PostgreSqlDatabaseOptions : SqlDatabaseOptions
    {
        public PostgreSqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = string.Empty;
            this.IsUsingSchemas = true;
        }
    }
}