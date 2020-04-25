using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;

namespace RecShark.Extensions.Testing.NSubstitute
{
    public static class LogExtensions
    {
        public static void Logged(this ILogger logger, LogLevel? level = null, string message = null, int count = 1)
        {
            logger.Logged(null, level, message, count);
        }

        public static void Logged(this ILogger logger, Exception exception, LogLevel? level = null,
            string message = null, int count = 1)
        {
            logger.Received(count)
                .Log(
                    level ?? Arg.Any<LogLevel>(),
                    Arg.Any<EventId>(),
                    message != null ? new FormattedLogValuesComparable(message) : Arg.Any<object>(),
                    exception,
                    Arg.Any<Func<object, Exception, string>>());
        }

        public static void DidNotLog(this ILogger logger, LogLevel? level = null, string message = null,
            Exception exception = null)
        {
            logger.Logged(exception, level, message, 0);
        }

        private class FormattedLogValuesComparable
        {
            public FormattedLogValuesComparable(string message)
            {
                Message = message;
            }

            public string Message { get; }

            public override bool Equals(object obj)
            {
                return obj is FormattedLogValues other && other.ToString() == Message;
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