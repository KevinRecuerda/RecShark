using System;
using RecShark.Testing;

namespace RecShark.Extensions.DependencyInjection.Testing
{
    public class IntegrationTests<T> : Tests, IDisposable
        where T : Hooks, new()
    {
        protected T Hooks { get; set; }

        protected IntegrationTests(Hooks hooks = null)
        {
            Hooks = HooksFactory.BuildHooks<T>(hooks);
        }

        public virtual void Dispose() { Hooks.Dispose();}
    }
}