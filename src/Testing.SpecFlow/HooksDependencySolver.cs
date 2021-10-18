using RecShark.Extensions.DependencyInjection.Testing;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow
{
    public abstract class HooksDependencySolver : Steps
    {
        private readonly Hooks   hooks;
        private readonly Hooks[] dependencies;

        protected HooksDependencySolver(Hooks hooks, params Hooks[] dependencies)
        {
            this.hooks        = hooks;
            this.dependencies = dependencies;
        }

        [BeforeScenario]
        public void MergeHooks()
        {
            foreach (var dependency in dependencies)
                dependency.Services = hooks.Services;
        }
    }
}