namespace RecShark.AspNetCore.Testing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerExtensionsForTests
    {
        public static void MockUser(this ControllerBase controller, string user = null, params string[] scopes)
        {
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            controller.ControllerContext.HttpContext.MockUser(user, scopes);
        }

        public static void MockUserScopes(this ControllerBase controller, params string[] scopes)
        {
            controller.MockUser(null, scopes);
        }
    }
}
