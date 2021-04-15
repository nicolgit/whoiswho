using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhoIsWho.Portal.Api.Services;

namespace WhoIsWho.Api
{
    public class ResultByTextFunction
    {
        private readonly CognitiveSearchService searchService;

        public ResultByTextFunction(CognitiveSearchService s)
        {
            searchService = s;
        }

        [FunctionName("ResultByText")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var searchString = req.Query["search"];
            var json = await searchService.ResultByText(searchString);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
