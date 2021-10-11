using Microsoft.AspNetCore.Mvc;

namespace RecShark.AspNetCore.Extensions.Extensions
{
    public class DefaultRouteAttribute : RouteAttribute
    {
        private const string DefaultTemplate = "api/v{version:apiVersion}/[controller]";

        public DefaultRouteAttribute() : base(DefaultTemplate) { }
    }
}
