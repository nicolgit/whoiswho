using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Threading.Tasks;
using WhoIsWho.Portal.Api.Services;

namespace WhoIsWho.Api
{
    public class CountFunction
    {
        private readonly AuthenticationService authService;
        private readonly CognitiveSearchService searchService;

        public CountFunction(AuthenticationService a, CognitiveSearchService s)
        {
            authService = a;
            searchService = s;
        }

        [FunctionName("Count")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var accessToken = authService.GetAccessToken(req);
            var claimsPrincipal = await authService.ValidateAccessToken(accessToken);
            if (claimsPrincipal != null)
            {
                var json = await searchService.Count();
                return (ActionResult)new OkObjectResult(new JsonResult(json));
            }
            else
            {
                return (ActionResult)new UnauthorizedResult();
            }  
        }
    }
}
