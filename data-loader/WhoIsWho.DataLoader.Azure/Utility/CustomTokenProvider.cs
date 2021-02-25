using Microsoft.Identity.Client;
using Microsoft.Rest;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace WhoIsWho.DataLoader.Azure.Utility
{
    class CustomTokenProvider : ITokenProvider
    {
        private readonly IConfidentialClientApplication _clientApp;
        private readonly string _scope;

        public CustomTokenProvider(IConfidentialClientApplication clientApp, string scope)
        {
            _clientApp = clientApp;
            _scope = scope;
        }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken)
        {
            var authenticationResult = await _clientApp
                .AcquireTokenForClient(new List<string> { _scope })
                .ExecuteAsync();

            return new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        }
    }
}
