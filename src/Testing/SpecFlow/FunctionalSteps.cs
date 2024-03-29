﻿using RecShark.Testing.DependencyInjection;

namespace RecShark.Testing.SpecFlow
{
    public abstract class FunctionalSteps<T> : IntegrationSteps<T>
        where T : FunctionalHooks, new()
    {
        protected FunctionalSteps(T hooks = null) : base(hooks) { }
    }
}
