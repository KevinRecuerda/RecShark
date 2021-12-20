namespace RecShark.AspNetCore.HttpClient.Security
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;

    public static class HttpClientExtensions
    {
        public static void SetAuthorization(this HttpClient client, SecurityConfig config)
        {
            switch (config?.Type)
            {
                case SecurityConfig.SecurityType.Basic:
                    client.SetBasicAuthorization(config.UserName, config.Password);
                    break;
                case SecurityConfig.SecurityType.Custom:
                    client.SetCustomAuthorization(config.UserName, config.Password);
                    break;
            }
        }

        public static void SetBasicAuthorization(this HttpClient client, string userName, string password)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        public static void SetCustomAuthorization(this HttpClient client, string userName, string password)
        {
            client.DefaultRequestHeaders.Add("Login",    userName);
            client.DefaultRequestHeaders.Add("Password", password);
        }

        public static void SetBearerAuthorization(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.SetBearerAuthorization(token);
        }

        public static void SetBearerAuthorization(this HttpRequestHeaders headers, string token)
        {
            headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
