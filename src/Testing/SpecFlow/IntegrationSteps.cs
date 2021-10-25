using RecShark.Extensions;
using RecShark.Testing.DependencyInjection;
using TechTalk.SpecFlow;

namespace RecShark.Testing.SpecFlow
{
    public abstract class IntegrationSteps<T> : Steps
        where T : Hooks, new()
    {
        static IntegrationSteps()
        {
            typeof(Tests).RunStaticConstructor();
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
