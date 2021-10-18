using Microsoft.Extensions.DependencyInjection;
using RecShark.Data.Db.Relational.Testing;
using RecShark.DependencyInjection;
using RecShark.Testing;

namespace RecShark.Data.Db.Relational.Tests
{
    public class BaseRelTests : IntegrationTests<RelHooks> { }

    public class RelHooks : RelationalDataHooks
    {
        public RelHooks() : base(new RelModule()) { }
    }

    public class RelModule : DIModule
    {
        public override DIModule[] Dependencies => new DIModule[] {new RelationalDataModule()};

        public override void Load(IServiceCollection services)
        {
            services.AddTransient<ISizeDataAccess, SizeDataAccess>();
        }
    }

    public class SampleBaseDataAccess : PostgreDataAccess
    {
        public SampleBaseDataAccess(IDbConnectionFactory dbConnectionFactory) : base(dbConnectionFactory) { }

        protected override string Name => "sample";
    }
}