using System.Globalization;
using RecShark.Extensions;
using RecShark.Testing.DependencyInjection;

namespace RecShark.Testing.SpecFlow
{
    public abstract class IntegrationSteps<T> : BasicSteps
        where T : Hooks, new()
    {
        protected T Hooks { get; set; }

        protected IntegrationSteps(T hooks = null)
        {
            Hooks = HooksFactory.BuildHooks<T>(hooks);
            CultureInfo.InvariantCulture.UseDefault();
        }
    }
}
