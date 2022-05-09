using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RecShark.Extensions;

namespace RecShark.Testing.NSubstitute
{
    public abstract class TestingLogger<T> : TestingLogger, ILogger<T>
    {
    }

    // partial credit: https://github.com/nsubstitute/NSubstitute/issues/597#issuecomment-653555567
    public abstract class TestingLogger : ILogger
    {
        public const string ScopePropertyDelimiter = "__";

        private AsyncLocal<List<TestingScope>> scopes = new AsyncLocal<List<TestingScope>>();

        public List<TestingScope> Scopes
        {
            get
            {
                scopes.Value ??= new List<TestingScope>();
                return scopes.Value;
            }
        }

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var scope = Scopes.Where(s => !s.IsDisposed && s.Scope != null)
                              .Select(s => s.Scope)
                              .Aggregate(new Dictionary<string, object>(), (all, x) => all.Apply(x));

            var template = (state as IReadOnlyList<KeyValuePair<string, object>> ?? Array.Empty<KeyValuePair<string, object>>())
                           .Where(x => x.Key != "{OriginalFormat}")
                           .ToDictionary(x => x.Key, x => x.Value);

            var fullScope = new Dictionary<string, object>().Apply(scope)
                                                            .Apply(template)
                                                            .Select(x => x.Value.Keying(x.Key))
                                                            .ToString(ScopePropertyDelimiter);

            // note: RenderedPattern is used to match generic log method call.
            Log(exception,
                logLevel,
                new RenderedPattern(fullScope, false),
                new RenderedPattern(formatter(state, exception), false));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var scope = new TestingScope() {Scope = state as Dictionary<string, object>};
            scopes.Value = Scopes.Concat(scope.InList()).ToList(); // combine & create new list, to avoid collision caused by shallow clone of execution context
            return scope;
        }

        public void Logged(LogLevel level, string message = null, int count = 1)
        {
            this.Logged(null, level, message, count);
        }

        public void Logged(Exception exception, LogLevel level, string message = null, int count = 1)
        {
            this.ReceivedLog(exception, level, null, message, count);
        }

        public void LoggedScope(LogLevel level, string scope, string message = null, int count = 1)
        {
            this.LoggedScope(null, level, scope, message, count);
        }

        public void LoggedScope(Exception exception, LogLevel level, string scope, string message = null, int count = 1)
        {
            this.ReceivedLog(exception, level, scope, message, count);
        }

        public void DidNotLog(LogLevel level, string message = null)
        {
            this.DidNotLog(null, level, message);
        }

        public void DidNotLog(Exception exception, LogLevel level, string message = null)
        {
            this.ReceivedLog(exception, level, null, message, 0);
        }

        protected void ReceivedLog(Exception exception, LogLevel level, string scope = null, string message = null, int count = 1)
        {
            // note: RenderedPattern is used instead of Arg.Is<string>(x => x.SmartMatchAny(message)), in order to have better debug msg.
            this.Received(count).Log(exception, level, new RenderedPattern(scope), new RenderedPattern(message));
        }

        protected abstract void Log(Exception exception, LogLevel logLevel, RenderedPattern scope, RenderedPattern message);
    }

    // used to render string correctly on output
    public class RenderedPattern
    {
        public static readonly string EscapePattern = "([" + "[](){}|+^$#".Select(x => @"\" + x).ToString("|") + "])";

        public RenderedPattern(string wildcardPattern, bool isPattern = true)
        {
            WildcardPattern = wildcardPattern;
            IsPattern       = isPattern;
        }

        public string WildcardPattern { get; }
        public bool   IsPattern       { get; }

        public override bool Equals(object obj)
        {
            var text = obj is RenderedPattern otherPattern
                ? otherPattern.WildcardPattern
                : obj?.ToString() ?? "";
            var safePattern = WildcardPattern != null ? Regex.Replace(WildcardPattern, EscapePattern, @"\$1") : null;
            return text.SmartMatchAny(safePattern);
        }

        public override int GetHashCode() => WildcardPattern?.GetHashCode() ?? -1;

        public override string ToString() => WildcardPattern != null
            ? WildcardPattern.Quoting("\"").Prefixing(IsPattern ? "~" : "", false)
            : "<null>";
    }

    public class TestingScope : IDisposable
    {
        public Dictionary<string, object> Scope { get; set; }

        public bool IsDisposed { get; set; }

        public void Dispose() => this.IsDisposed = true;
    }
}
