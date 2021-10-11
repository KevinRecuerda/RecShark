using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace RecShark.Extensions.DependencyInjection.Testing
{
    public class Hooks : IDisposable
    {
        public Hooks(params DIModule[] modules)
        {
            this.Services = new ServiceCollection();
            this.Services.Load(modules);

            var configuration = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json",             true)
                               .AddJsonFile("appsettings.Development.json", true)
                               .Build();
            
            this.Services.AddSingleton<IConfiguration>(configuration);

            if (this.Services.Count(x => x.ServiceType == typeof(ILoggerFactory)) == 0)
            {
                var factory = new SubstituteLoggerFactory();
                this.Services.AddSingleton<ILoggerFactory>(factory);
            }

            if (this.Services.Count(x => x.ServiceType == typeof(ILogger<>)) == 0)
            {
                this.Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            }

            this.BuildProvider();
        }

        public void BuildProvider()
        {
            this.Provider = this.Services.BuildServiceProvider();
        }

        public IServiceCollection Services { get; set; }
        public IServiceProvider   Provider { get; private set; }

        public ILogger GetLogger<T>()
        {
            var factory = this.Provider.GetService<ILoggerFactory>();
            var logger  = factory.CreateLogger(typeof(T));
            return logger;
        }

        public virtual void Dispose() { }
    }

    public class SubstituteLoggerFactory : ILoggerFactory
    {
        public ConcurrentDictionary<string, ILogger> Loggers { get; set; } = new ConcurrentDictionary<string, ILogger>();

        public ILogger CreateLogger(string categoryName)
        {
            if (!this.Loggers.ContainsKey(categoryName))
                this.Loggers[categoryName] = Substitute.For<ILogger>();

            return this.Loggers[categoryName];
        }

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose()
        {
            this.Loggers.Clear();
        }
    }
}