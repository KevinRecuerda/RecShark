using RecShark.DependencyInjection.Tests;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow.Tests.DependencyInjection
{
    [Binding]
    public class HooksSolver : HooksDependencySolver
    {
        public HooksSolver(ProjectHooks hooks, SharedHooks sharedHooks) : base(hooks, sharedHooks) { }
    }

    public interface ISampleApiClient : ISample { }

    public class SampleApiClient : ISampleApiClient
    {
        public string Hello()
        {
            return "should be substitute automatically";
        }
    }
}
