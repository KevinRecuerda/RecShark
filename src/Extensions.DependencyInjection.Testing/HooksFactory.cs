namespace RecShark.Extensions.DependencyInjection.Testing
{
    public static class HooksFactory
    {
        public static T BuildHooks<T>(Hooks hooks)
            where T : Hooks, new()
        {
            var newHooks = BuildHooksInner<T>(hooks);
            newHooks.BuildProvider();
            return newHooks;
        }

        private static T BuildHooksInner<T>(Hooks hooks)
            where T : Hooks, new()
        {
            switch (hooks)
            {
                case null:   return new T();
                case T same: return same;
                default:     return new T() {Services = hooks.Services};
            }
        }
    }
}
