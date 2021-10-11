using RecShark.Testing.SpecFlow;
using TechTalk.SpecFlow;

namespace RecShark.Extensions.DependencyInjection.Tests.SpecFlow
{
    [Binding]
    public class HooksSolver : HooksDependencySolver
    {
        public HooksSolver(ProjectHooks hooks, SharedHooks sharedHooks) : base(hooks, sharedHooks) { }
    }

    public class SampleBaseApiClient : ISample
    {
        public string Hello()
        {
            return "should be substitute";
        }
    }
}