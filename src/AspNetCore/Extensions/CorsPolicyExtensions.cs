namespace RecShark.AspNetCore.Extensions
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Cors.Infrastructure;

    public static class CorsPolicyExtensions
    {
        public const string WildcardSubDomain = "*.";

        public static CorsPolicyBuilder SetSubDomainsAllowed(this CorsPolicyBuilder builder, bool strictPort = false)
        {
            var policy = builder.Build();
            return builder.SetIsOriginAllowed(o => policy.IsOriginAnAllowedSubDomain(o, strictPort));
        }

        public static bool IsOriginAnAllowedSubDomain(this CorsPolicy policy, string origin, bool strictPort = false)
        {
            if (policy.Origins.Contains(origin))
                return true;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            return policy.Origins
                         .Where(o => o.Contains("://" + WildcardSubDomain))
                         .Select(o => new Uri(o.Replace(WildcardSubDomain, ""), UriKind.Absolute))
                         .Any(domain => uri.IsSubDomain(domain));
        }

        public static bool IsSubDomain(this Uri subDomain, Uri domain, bool strictPort = false)
        {
            return subDomain.IsAbsoluteUri && domain.IsAbsoluteUri && subDomain.Scheme == domain.Scheme
                && (!strictPort || subDomain.Port == domain.Port)
                && subDomain.Host.EndsWith("." + domain.Host, StringComparison.Ordinal);
        }
    }
}
