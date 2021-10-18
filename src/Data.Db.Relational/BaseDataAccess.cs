using System.Data;
using Dapper.FastCrud;

namespace RecShark.Data.Db.Relational
{
    public abstract class BaseDataAccess
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        protected BaseDataAccess(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        protected abstract string Type { get; }
        protected abstract string Name { get; }

        protected IDbConnection Open()
        {
            var dbConnection = dbConnectionFactory.CreateDbConnection(Type, Name);
            dbConnection.Open();
            return dbConnection;
        }

        protected void SetDialectForMapping<T>()
        {
            if (!OrmConfiguration.GetDefaultEntityMapping<T>().IsFrozen)
            {
                OrmConfiguration.GetDefaultEntityMapping<T>().SetDialect(dbConnectionFactory.GetSqlDialect(Type));
            }
        }
    }

    public abstract class PostgreDataAccess : BaseDataAccess
    {
        protected PostgreDataAccess(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        protected override string Type => "postgre";
    }

    public abstract class OracleDataAccess : BaseDataAccess
    {
        protected OracleDataAccess(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        protected override string Type => "oracle";
    }
}