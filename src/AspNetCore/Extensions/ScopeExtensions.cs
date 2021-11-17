namespace RecShark.AspNetCore.Extensions
{
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;

    public static class ScopeExtensions
    {
        public static AuthorizationPolicyBuilder RequireScopes(this AuthorizationPolicyBuilder builder, params string[] scopes)
        {
            foreach (var scope in scopes)
                builder = builder.RequireClaim("scope", scope);

            return builder;
        }

        public static void AddScopes(this ClaimsPrincipal claimsPrincipal, params string[] scopes)
        {
            ((ClaimsIdentity)claimsPrincipal.Identity).AddScopes(scopes);
        }

        public static void AddScopes(this ClaimsIdentity claimsIdentity, params string[] scopes)
        {
            var scopeClaims = scopes.Select(s => new Claim("scope", s)).ToList();
            claimsIdentity.AddClaims(scopeClaims);
        }
    }
}
