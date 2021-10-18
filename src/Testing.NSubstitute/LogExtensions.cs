using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;
using NSubstitute.Core;

namespace RecShark.Testing.NSubstitute
{
    public static class LogExtensions
    {
        public static void Logged(this ILogger logger, LogLevel? level = null, string wildcardExpression = null, int count = 1)
        {
            logger.Logged(null, level, wildcardExpression, count);
        }

        public static void Logged(
            this ILogger logger,
            Exception    exception,
            LogLevel?    level              = null,
            string       wildcardExpression = null,
            int          count              = 1)
        {
            var argException = SubstitutionContext.Current.ThreadContext.DequeueAllArgumentSpecifications().FirstOrDefault(x => x.ForType == typeof(Exception));

            var lvl = level ?? Arg.Any<LogLevel>();
            var eventId = Arg.Any<EventId>();
            var state = wildcardExpression != null ? new FormattedLogValuesComparable(wildcardExpression) : Arg.Any<object>();
            if (argException != null)
                SubstitutionContext.Current.ThreadContext.EnqueueArgumentSpecification(argException);
            
            logger.Received(count)
                  .Log(
                       lvl,
                       eventId,
                       state,
                       exception,
                       Arg.Any<Func<object, Exception, string>>());
        }        
        
        public static void DidNotLog(
            this ILogger logger,
            LogLevel?    level              = null,
            string       wildcardExpression = null,
            Exception    exception          = null)
        {
            logger.Logged(exception, level, wildcardExpression, 0);
        }
        
        private class FormattedLogValuesComparable
        {
            public FormattedLogValuesComparable(string wildcardExpression)
            {
                WildcardExpression = wildcardExpression;
            }

            public string WildcardExpression { get; }

            public override bool Equals(object obj)
            {
                return obj is FormattedLogValues other && Regex.IsMatch(other.ToString(), ConvertWildcardToRegEx(WildcardExpression), RegexOptions.Singleline);
            }

            public override int GetHashCode()
            {
                return WildcardExpression.GetHashCode();
            }

            public override string ToString()
            {
                return WildcardExpression;
            }

            private static string ConvertWildcardToRegEx(string wildcardExpression)
            {
                return "^" + Regex.Escape(wildcardExpression).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            }
        }
    }
}
