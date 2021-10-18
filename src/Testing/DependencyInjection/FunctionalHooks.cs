using System;
using System.Linq;
using RecShark.Extensions.DependencyInjection;

namespace RecShark.Testing.DependencyInjection
{
    public class FunctionalHooks : Hooks
    {
        public virtual Type[] SubstitutedServices => Type.EmptyTypes;

        public FunctionalHooks(params DIModule[] modules) : base(modules)
        {
            // TODO: create common interface to mock all external call (db/api)
            // data.db.relational
            // data.db.document
            // data.api
            Services.Substitute(x => x.InheritedFrom("BaseDataAccess", "BaseDocumentDataAccess", "BaseApiClient"));
            Services.Substitute(x => SubstitutedServices.Contains(x.ServiceType));
        }
    }
}
