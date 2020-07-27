using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;

namespace RecShark.Extensions.Testing.NSubstitute
{
    public static class LogExtensions
    {
        public static void Logged(this ILogger logger, LogLevel? level = null, string wildcardExpression = null, int count = 1)
        {
            logger.Logged(null, level, wildcardExpression, count);
        }

        public static void DidNotLog(
            this ILogger logger,
            LogLevel?    level              = null,
            string       wildcardExpression = null,
            Exception    exception          = null)
        {
            logger.Logged(exception, level, wildcardExpression, 0);
        }

        private static void Logged(
            this ILogger logger,
            Exception    exception,
            LogLevel?    level              = null,
            string       wildcardExpression = null,
            int          count              = 1)
        {
            logger.Received(count)
                  .Log(
                       level ?? Arg.Any<LogLevel>(),
                       Arg.Any<EventId>(),
                       wildcardExpression != null ? new FormattedLogValuesComparable(wildcardExpression) : Arg.Any<object>(),
                       exception,
                       Arg.Any<Func<object, Exception, string>>());
        }

        private class FormattedLogValuesComparable
        {
            public FormattedLogValuesComparable(string wildcardExpression)
            {
                this.WildcardExpression = wildcardExpression;
            }

            public string WildcardExpression { get; }

            public override bool Equals(object obj)
            {
                return obj is FormattedLogValues other && Regex.IsMatch(other.ToString(), ConvertWildcardToRegEx(this.WildcardExpression));
            }

            public override int GetHashCode()
            {
                return this.WildcardExpression.GetHashCode();
            }

            public override string ToString()
            {
                return this.WildcardExpression;
            }

            private static string ConvertWildcardToRegEx(string wildcardExpression)
            {
                return "^" + Regex.Escape(wildcardExpression).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            }
        }
    }
}
