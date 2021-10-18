using RecShark.Testing.DependencyInjection;
using RecShark.Testing.SpecFlow.Extensions;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow
{
    public abstract class IntegrationSteps<T> : Steps
        where T : Hooks, new()
    {
        static IntegrationSteps()
        {
            Tests.Setup();
            SpecFlowExtensions.UseObjectConverter();
        }

        protected T Hooks { get; set; }

        protected IntegrationSteps(Hooks hooks = null)
        {
            Hooks = HooksFactory.BuildHooks<T>(hooks);

            Tests.OverrideCultureInfo();
        }
    }
}
