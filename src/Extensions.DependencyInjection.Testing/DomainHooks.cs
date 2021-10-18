using System;
using System.Linq;

namespace RecShark.Extensions.DependencyInjection.Testing
{
    public class DomainHooks : Hooks
    {
        public virtual Type[] SubstitutedServices => new Type[0];

        public DomainHooks(params DIModule[] modules) : base(modules)
        {
            // TODO: create common interface
            // data.db.relational
            // data.db.document
            // data.api
            Services.Substitute(x => x.InheritedFrom("BaseDataAccess", "BaseDocumentDataAccess", "BaseApiClient"));
            Services.Substitute(x => SubstitutedServices.Contains(x.ServiceType));
        }
    }
}
