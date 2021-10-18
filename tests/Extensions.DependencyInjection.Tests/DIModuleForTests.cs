using Microsoft.Extensions.DependencyInjection;

namespace RecShark.Extensions.DependencyInjection.Tests
{
    public class DIModuleForTests : DIModule
    {
        public override void Load(IServiceCollection services)
        {
            CallCount++;
        }

        public int CallCount { get; private set; }
    }

    public class DIModuleA : DIModuleForTests
    {
        public override DIModule[] Dependencies => new DIModule[] {new DIModuleB(), new DIModuleC()};
    }

    public class DIModuleB : DIModuleForTests
    {
        public override DIModule[] Dependencies => new DIModule[] {new DIModuleC()};

        public override void Load(IServiceCollection services)
        {
            base.Load(services);
            services.Reset(ServiceDescriptor.Singleton<ISample, Sample>());
        }
    }

    public class DIModuleC : DIModuleForTests
    {
        public override void Load(IServiceCollection services)
        {
            base.Load(services);
            services.AddTransient<ISample, Sample>();
        }
    }
}