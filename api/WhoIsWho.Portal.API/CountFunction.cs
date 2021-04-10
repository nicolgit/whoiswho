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
        private readonly CognitiveSearchService searchService;

        public CountFunction(CognitiveSearchService s)
        {
            searchService = s;
        }

        [FunctionName("Count")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var json = await searchService.Count();
            return new JsonResult(json);
        }
    }
}
