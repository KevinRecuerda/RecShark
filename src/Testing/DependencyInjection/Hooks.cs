using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecShark.DependencyInjection;
using RecShark.Testing.NSubstitute;

namespace RecShark.Testing.DependencyInjection
{
    public class Hooks : IDisposable
    {
        public Hooks(params DIModule[] modules)
        {
            Services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json",             true)
                               .AddJsonFile("appsettings.Development.json", true)
                               .Build();
            Services.AddSingleton<IConfiguration>(configuration);

            Services.Load(modules);

            Services.Reset(ServiceDescriptor.Singleton<ILoggerFactory>(new SubstituteLoggerFactory()));
            Services.Reset(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

            BuildProvider();
        }

        public void BuildProvider()
        {
            Provider = Services.BuildServiceProvider();
        }

        public IServiceCollection Services { get; set; }
        public IServiceProvider   Provider { get; private set; }

        public TestingLogger GetLogger<T>()
        {
            var factory = Provider.GetService<ILoggerFactory>();
            var logger  = factory.CreateLogger(typeof(T));
            return logger as TestingLogger;
        }

        public virtual void Dispose() { }
    }
}
