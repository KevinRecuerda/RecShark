using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace RecShark.Testing.DependencyInjection
{
    public class SubstituteLoggerFactory : ILoggerFactory
    {
        public ConcurrentDictionary<string, ILogger> Loggers { get; set; } = new ConcurrentDictionary<string, ILogger>();

        public ILogger CreateLogger(string categoryName)
        {
            if (!Loggers.ContainsKey(categoryName))
                Loggers[categoryName] = Substitute.For<ILogger>();

            return Loggers[categoryName];
        }

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose()
        {
            Loggers.Clear();
        }
    }
}