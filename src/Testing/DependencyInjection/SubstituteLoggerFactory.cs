using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecShark.Testing.NSubstitute;

namespace RecShark.Testing.DependencyInjection
{
    public class SubstituteLoggerFactory : ILoggerFactory
    {
        public ConcurrentDictionary<string, TestingLogger> Loggers { get; set; } = new ConcurrentDictionary<string, TestingLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            if (!Loggers.ContainsKey(categoryName))
                Loggers[categoryName] = Substitute.For<TestingLogger>();

            return Loggers[categoryName];
        }

        public void AddProvider(ILoggerProvider provider) { }

        public void Dispose()
        {
            Loggers.Clear();
        }
    }
}
