namespace RecShark.AspNetCore.Testing
{
    using Microsoft.AspNetCore.Http;

    public static class HttpContextExtensionsForTests
    {
        public static void MockUser(this HttpContext httpContext, string user = null, params string[] scopes)
        {
            httpContext.User = PrincipalBuilderForTests.Default(user)
                                                       .WithScopes(scopes)
                                                       .Build();
        }
    }
}
