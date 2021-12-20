namespace RecShark.AspNetCore.HttpClient.Security
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class AccessTokenDelegatingHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider                  accessTokenProvider;
        private readonly ILogger<AccessTokenDelegatingHandler> logger;

        public AccessTokenDelegatingHandler(IAccessTokenProvider accessTokenProvider, ILogger<AccessTokenDelegatingHandler> logger)
        {
            this.accessTokenProvider = accessTokenProvider;
            this.logger              = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = await this.accessTokenProvider.GetAccessToken(cancellationToken);
            request.Headers.SetBearerAuthorization(accessToken);
            this.logger.LogDebug($"[OAUTH2] use token: {accessToken}");
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
