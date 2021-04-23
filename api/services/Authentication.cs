using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace WhoIsWho.Portal.Api.Services
{
    public class AuthenticationService
    {
        private readonly ILogger logger;
		private readonly IConfiguration config;
        private readonly string audience; // Get this value from the expose an api, audience uri section example https://appname.tenantname.onmicrosoft.com
        private readonly string clientID; // this is the client id, also known as AppID. This is not the ObjectID
        private readonly string tenant; // this is your tenant name "<tenantname>.onmicrosoft.com"
        private readonly string tenantid; // this is your tenant id (GUID)

        // rest of the values below can be left as is in most circumstances
        private string aadInstance = "https://login.microsoftonline.com/{0}/v2.0";
        private string authority;
        private List<string> validIssuers;

        public AuthenticationService(
            ILogger<AuthenticationService> loggerIn,
            IConfiguration configIn)
        {
            logger = loggerIn;
            config = configIn;

            audience = config["OIDC_AUDIENCE"];
            clientID = config["OIDC_CLIENTID"];
            tenant = config["OIDC_TENANT"];
            tenantid = config["OIDC_TENANTID"];

            authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            validIssuers = new List<string>()
            {
                $"https://login.microsoftonline.com/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0",
                $"https://login.windows.net/{tenant}/",
                $"https://login.microsoft.com/{tenant}/",
                $"https://sts.windows.net/{tenantid}/"
            };
        }

        public string GetAccessToken(HttpRequest req)
            {
                var authorizationHeader = req.Headers?["Authorization"];
                string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
                if (parts.Length == 2 && parts[0].Equals("Bearer"))
                    return parts[1];
                return null;
            }

        public async Task<ClaimsPrincipal> ValidateAccessToken(string accessToken)
            {
                // Debugging purposes only, set this to false for production
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

                var url = $"{authority}/.well-known/openid-configuration";
                //var url = $"{authority}/common/.well-known/openid-configuration";
                

                ConfigurationManager<OpenIdConnectConfiguration> configManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        url,
                        new OpenIdConnectConfigurationRetriever());

                OpenIdConnectConfiguration config = null;
                config = await configManager.GetConfigurationAsync();

                ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

                // Initialize the token validation parameters
                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    // App Id URI and AppId of this service application are both valid audiences.
                    ValidateAudience = true,
                    ValidAudiences = new[] { audience, clientID },

                    // Support Azure AD V1 and V2 endpoints.
                    ValidateIssuer = true,
                    ValidIssuers = validIssuers,
                    
                    IssuerSigningKeys = config.SigningKeys,
                    ValidateIssuerSigningKey= true
                };

                try
                {
                    SecurityToken securityToken;
                    var claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);
                    return claimsPrincipal;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                }
                return null;
            }
    }
}