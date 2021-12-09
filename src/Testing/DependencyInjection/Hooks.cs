using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RecShark.DependencyInjection;

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

            if (Services.Count(x => x.ServiceType == typeof(ILoggerFactory)) == 0)
            {
                var factory = new SubstituteLoggerFactory();
                Services.AddSingleton<ILoggerFactory>(factory);
            }

            if (Services.Count(x => x.ServiceType == typeof(ILogger<>)) == 0)
            {
                Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            }

            BuildProvider();
        }

        public void BuildProvider()
        {
            Provider = Services.BuildServiceProvider();
        }

        public IServiceCollection Services { get; set; }
        public IServiceProvider   Provider { get; private set; }

        public ILogger GetLogger<T>()
        {
            var factory = Provider.GetService<ILoggerFactory>();
            var logger  = factory.CreateLogger(typeof(T));
            return logger;
        }

        public virtual void Dispose() { }
    }
}
