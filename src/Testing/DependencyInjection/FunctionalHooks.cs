using System;
using System.Linq;
using RecShark.DependencyInjection;
using RecShark.Extensions;

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
                Services.Substitute(x => x.ServiceType.IsInterface && x.ServiceType.AsFirstName().EndsWith(externalConnector));
            }

            Services.Substitute(x => SubstitutedServices.Contains(x.ServiceType));
        }
    }
}
