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
            var externalConnectors = new[] { "DataAccess", "ApiClient", "Connector" };
            foreach (var externalConnector in externalConnectors)
            {
                Services.Substitute(x => x.ServiceType.Name.EndsWith(externalConnector) && x.ServiceType.IsInterface);
            }

            Services.Substitute(x => SubstitutedServices.Contains(x.ServiceType));
        }
    }
}
