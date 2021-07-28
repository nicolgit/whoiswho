using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhoIsWho.Portal.Api.Services;

namespace WhoIsWho.Portal.API
{
    public class SuggestFunction
    {
        private readonly AuthenticationService authService;
        private readonly CognitiveSearchService searchService;

        public SuggestFunction(AuthenticationService a, CognitiveSearchService s)
        {
            authService = a;
            searchService = s;
        }

        [FunctionName("Suggest")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var accessToken = authService.GetAccessToken(req);
            var claimsPrincipal = await authService.ValidateAccessToken(accessToken);
            if (claimsPrincipal != null)
            {
                var searchString = req.Query["s-earch"];
                var json= await searchService.Suggest(searchString);
                
                return (ActionResult)new OkObjectResult(json);
            }
            else
            {
                return (ActionResult)new UnauthorizedResult();
            }

            
        }
    }
}
