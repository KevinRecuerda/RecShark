namespace RecShark.AspNetCore.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;

    public class PrincipalBuilderForTests
    {
        public const string Name = "user-test";
        public const string Mail = "user@mail.com";
        public const string Role = "my-role";

        private readonly ClaimsIdentity identity;
        private          List<string>   principalRoles = new List<string>();

        public static Func<string, PrincipalBuilderForTests> Default = name => Create(name)
                                                                              .WithMail(Mail)
                                                                              .WithRoles(Role);

        public static PrincipalBuilderForTests Create(string name = null) => new PrincipalBuilderForTests(name ?? Name);

        private PrincipalBuilderForTests(string name)
        {
            this.identity = new ClaimsIdentity(new GenericIdentity(name));
            this.WithName(name);
        }

        public PrincipalBuilderForTests WithName(string name)
        {
            return this.WithClaim("name", name);
        }

        public PrincipalBuilderForTests WithMail(string mail)
        {
            return this.WithClaim("mail", mail);
        }

        public PrincipalBuilderForTests WithScopes(params string[] scopes)
        {
            var claims = scopes.Select(s => ("scope", s)).ToArray();
            return this.WithClaims(claims);
        }

        public PrincipalBuilderForTests WithRoles(params string[] roles)
        {
            this.principalRoles = roles.ToList();
            return this;
        }

        public PrincipalBuilderForTests WithClaim(string type, string value)
        {
            return this.WithClaims((type, value));
        }

        public PrincipalBuilderForTests WithClaims(params (string, string)[] claims)
        {
            return this.WithClaims(claims.Select(x => new Claim(x.Item1, x.Item2)).ToArray());
        }

        public PrincipalBuilderForTests WithClaims(params Claim[] claims)
        {
            this.identity.AddClaims(claims);
            return this;
        }

        public GenericPrincipal Build() => new GenericPrincipal(this.identity, this.principalRoles.ToArray());
    }
}
