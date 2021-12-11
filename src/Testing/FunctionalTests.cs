using RecShark.Testing.DependencyInjection;

namespace RecShark.Testing
{
    public class FunctionalTests<T> : IntegrationTests<T>
        where T : FunctionalHooks, new()
    {
        protected FunctionalTests(FunctionalHooks hooks = null) : base(hooks) { }
    }
}
