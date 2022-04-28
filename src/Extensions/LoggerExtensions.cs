using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RecShark.Extensions
{
    public static class LoggerExtensions
    {
        public static IDisposable WithScope(this ILogger logger, params (string, object)[] properties)
        {
            var dictionary = properties.ToDictionary(p => p.Item1, p => p.Item2);
            return logger.BeginScope(dictionary);
        }
    }
}
