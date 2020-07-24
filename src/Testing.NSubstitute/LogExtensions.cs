using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;

namespace RecShark.Extensions.Testing.NSubstitute
{
    using System.Text.RegularExpressions;

    public static class LogExtensions
    {
        public static void Logged(this ILogger logger, LogLevel? level = null, string message = null, int count = 1)
        {
            logger.Logged(null, level, FormattedLogValuesComparable.Is(message), count);
        }

        public static void LoggedMatching(this ILogger logger, LogLevel? level = null, string pattern = null, int count = 1)
        {
            logger.Logged(null, level, FormattedLogValuesComparable.Matchs(pattern), count);
        }

        public static void DidNotLog(
            this ILogger logger,
            LogLevel?    level     = null,
            string       message   = null,
            Exception    exception = null)
        {
            logger.Logged(exception, level, FormattedLogValuesComparable.Is(message), 0);
        }

        public static void DidNotLogMatching(
            this ILogger logger,
            LogLevel?    level     = null,
            string       pattern   = null,
            Exception    exception = null)
        {
            logger.Logged(exception, level, FormattedLogValuesComparable.Matchs(pattern), 0);
        }

        private static void Logged(
            this ILogger                 logger,
            Exception                    exception,
            LogLevel?                    level              = null,
            FormattedLogValuesComparable formattedLogValues = null,
            int                          count              = 1)
        {
            logger.Received(count)
                  .Log(
                       level ?? Arg.Any<LogLevel>(),
                       Arg.Any<EventId>(),
                       formattedLogValues ?? Arg.Any<object>(),
                       exception,
                       Arg.Any<Func<object, Exception, string>>());
        }

        private class FormattedLogValuesComparable
        {
            public static FormattedLogValuesComparable Is(string message)
            {
                return message != null
                           ? new FormattedLogValuesComparable(message, (s1, s2) => s1 == s2)
                           : null;
            }

            public static FormattedLogValuesComparable Matchs(string pattern)
            {
                return pattern != null
                           ? new FormattedLogValuesComparable(pattern, (s1, s2) => new Regex(pattern).IsMatch(s1.ToString()))
                           : null;
            }

            public FormattedLogValuesComparable(string message, Func<string, string, bool> equalityComparer)
            {
                Message          = message;
                EqualityComparer = equalityComparer;
            }

            public Func<string, string, bool> EqualityComparer;
            public string                     Message { get; }

            public override bool Equals(object obj)
            {
                return obj is FormattedLogValues other && EqualityComparer(other.ToString(), this.Message);
            }

            public override int GetHashCode()
            {
                return Message.GetHashCode();
            }

            public override string ToString()
            {
                return Message;
            }
        }
    }
}
