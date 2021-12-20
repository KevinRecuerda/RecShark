namespace RecShark.AspNetCore.HttpClient
{
    public class ApiClientConfig
    {
        public string Code { get; set; }
        public string Url  { get; set; }

        public bool Enabled { get; set; } = true;

        public SecurityConfig Security { get; set; }
        public ProxyConfig    Proxy    { get; set; }
    }

    public class SecurityConfig
    {
        public string Type { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }

        public string[] Scopes            { get; set; }
        public string   AuthorityTokenUrl { get; set; }

        public static class SecurityType
        {
            public const string Oauth2 = "oauth2";
            public const string Basic  = "basic";
            public const string Custom = "custom";
        }
    }

    public class ProxyConfig
    {
        public string Url      { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
