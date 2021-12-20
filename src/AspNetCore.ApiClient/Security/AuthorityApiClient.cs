namespace RecShark.AspNetCore.ApiClient.Security
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using RecShark.Extensions;

    public interface IAuthorityApiClient
    {
        Task<Token> RequestToken(SecurityConfig securityConfig, CancellationToken cancellationToken);
    }

    public sealed class Token
    {
        public string         AccessToken { get; set; }
        public DateTimeOffset Expiration  { get; set; }

        public override string ToString()
        {
            return $"{this.AccessToken} (expiration={this.Expiration})";
        }
    }

    public class AuthorityApiClient : IAuthorityApiClient
    {
        public async Task<Token> RequestToken(SecurityConfig securityConfig, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.SetBasicAuthorization(securityConfig.UserName, securityConfig.Password);

            using var request = new HttpRequestMessage(HttpMethod.Post, securityConfig.AuthorityTokenUrl)
            {
                Content = new FormUrlEncodedContent(
                    new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("scope",      securityConfig.Scopes.ToString(" "))
                    })
            };
            using var response = await httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
            var json    = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(json);

            var token = new Token()
            {
                AccessToken = jObject["access_token"].Value<string>(),
                Expiration  = DateTimeOffset.UtcNow.AddSeconds(jObject["expires_in"].Value<int>())
            };
            return token;
        }
    }
}
