using System;
using System.Linq;
using RecShark.DependencyInjection;

namespace RecShark.Testing.DependencyInjection
{
    public class FunctionalHooks : Hooks
    {
        public virtual Type[] SubstitutedServices => Type.EmptyTypes;

        public FunctionalHooks(params DIModule[] modules) : base(modules)
        {
            Services.Substitute(x => x.ServiceType.Name.EndsWith("DataAccess"));
            Services.Substitute(x => x.ServiceType.Name.EndsWith("ApiClient"));
            Services.Substitute(x => x.ServiceType.Name.EndsWith("Connector"));
            Services.Substitute(x => SubstitutedServices.Contains(x.ServiceType));
        }
    }
}
