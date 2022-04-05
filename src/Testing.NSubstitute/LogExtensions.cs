using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;

namespace RecShark.Testing.NSubstitute
{
    using System.Collections.Generic;

    public static class LogExtensions
    {
        public static void Logged(this ILogger logger, LogLevel? level = null, string wildcardExpression = null, int count = 1)
        {
            logger.Logged(null, level, wildcardExpression, count);
        }

        public static void Logged(this ILogger logger, Exception exception, LogLevel? level = null, string wildcardExpression = null, int count = 1)
        {
            var argException = SubstitutionContext.Current.ThreadContext.DequeueAllArgumentSpecifications()
                                                  .FirstOrDefault(x => x.ForType == typeof(Exception));

            logger.AdaptCalls();

            var lvl     = level ?? Arg.Any<LogLevel>();
            var eventId = Arg.Any<EventId>();
            var state   = wildcardExpression != null ? new FormattedLogValuesComparable(wildcardExpression) : Arg.Any<object>();
            if (argException != null)
                SubstitutionContext.Current.ThreadContext.EnqueueArgumentSpecification(argException);

            logger.Received(count)
                  .Log(lvl, eventId, state, exception, Arg.Any<Func<object, Exception, string>>());
        }

        public static void DidNotLog(this ILogger logger, LogLevel? level = null, string wildcardExpression = null, Exception exception = null)
        {
            logger.Logged(exception, level, wildcardExpression, 0);
        }

        public static void LoggedInOrder(this ILogger logger, Action<ILogger> calls)
        {
            logger.AdaptCalls();
            Received.InOrder(() => { calls(logger); });
        }

        public static void AdaptCalls(this ILogger logger)
        {
            var calls        = logger.ReceivedCalls().ToArray();
            var callsToAdapt = calls.Where(call => call.GetMethodInfo().ToString().Contains("Log[FormattedLogValues]")).ToList();
            if (callsToAdapt.Count == 0)
                return;

            var beginScopeCalls = calls.Where(call => call.GetMethodInfo().ToString().Contains("BeginScope")).ToList();
            //var scopeProperties = beginScopeCalls.SelectMany(c => c.GetArguments().FirstOrDefault() as Dictionary<string, string>).ToList();


            logger.ClearReceivedCalls();
            foreach (var call in callsToAdapt)
                ReCall(logger, call);
        }

        private static void ReCall(ILogger logger, ICall call)
        {
            var args      = call.GetArguments();
            var logLevel  = (LogLevel) args[0];
            var eventId   = (EventId) args[1];
            var message   = (args[2] as FormattedLogValuesComparable) ?? new FormattedLogValuesComparable(args[2].ToString());
            var exception = (Exception) args[3];

            logger.Log<object>(logLevel, eventId, message, exception, (state, ex) => state.ToString());
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
                var regex = BuildRegex(this.WildcardExpression);
                return Regex.IsMatch(obj?.ToString(), regex, RegexOptions.Singleline);
            }

            public override int GetHashCode()
            {
                return WildcardExpression.GetHashCode();
            }

            public override string ToString()
            {
                return WildcardExpression;
            }

            private static string BuildRegex(string wildcardExpression)
            {
                return "^" + Regex.Escape(wildcardExpression).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            }
        }
    }
}
