using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly AsyncLocal<List<TestingScope>> scopes = new AsyncLocal<List<TestingScope>>();

        public static AsyncLocal<List<TestingScope>> Scopes
        {
            get
            {
                scopes.Value ??= new List<TestingScope>();
                return scopes;
            }
        }

        public virtual bool IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var scope = Scopes.Value.Where(s => !s.IsDisposed && s.Scope != null)
                              .Select(s => s.Scope)
                              .Aggregate(new Dictionary<string, object>(), (all, x) => all.Apply(x))
                              .Select(x => x.Value.Keying(x.Key))
                              .ToString(ScopePropertyDelimiter);

            // note: RenderedPattern is used to match generic log method call.
            Log(exception,
                logLevel,
                new RenderedPattern(formatter(state, exception), false),
                new RenderedPattern(scope, false));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var scope = new TestingScope() {Scope = state as Dictionary<string, object>};
            Scopes.Value.Add(scope);
            return scope;
        }

        public void Logged(LogLevel level, string message = null, string scope = null, int count = 1)
        {
            this.Logged(null, level, message, scope, count);
        }

        public void Logged(Exception exception, LogLevel level, string message = null, string scope = null, int count = 1)
        {
            // note: RenderedPattern is used instead of Arg.Is<string>(x => x.SmartMatchAny(message)), in order to have better debug msg.
            this.Received(count).Log(exception, level, new RenderedPattern(message), new RenderedPattern(scope));
        }

        public void DidNotLog(LogLevel level, string message = null, string scope = null)
        {
            this.DidNotLog(null, level, message, scope);
        }

        public void DidNotLog(Exception exception, LogLevel level, string message = null, string scope = null)
        {
            this.Logged(exception, level, message, scope, 0);
        }

        protected abstract void Log(Exception exception, LogLevel logLevel, RenderedPattern message, RenderedPattern scope);
    }

    // used to render string correctly on output
    public class RenderedPattern
    {
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
            return text.SmartMatchAny(WildcardPattern);
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
