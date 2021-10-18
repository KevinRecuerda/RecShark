namespace RecShark.Testing.DependencyInjection
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
            return hooks switch
            {
                null => new T(),
                T same => same,
                _ => new T() { Services = hooks.Services }
            };
        }
    }
}