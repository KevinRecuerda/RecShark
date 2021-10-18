using Dapper.FastCrud;
using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Relational.Orm;
using RecShark.DependencyInjection;

namespace RecShark.Data.Db.Relational
{
    public class RelationalDataModule : DIModule
    {
        public override void Load(IServiceCollection services)
        {
            OrmConfiguration.DefaultDialect = SqlDialect.PostgreSql;
            OrmConfiguration.Conventions    = new PostgreOrmConventions();

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            services.AddSingleton<IConnectionStrings, ConnectionStrings>();
            services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();
        }
    }
}