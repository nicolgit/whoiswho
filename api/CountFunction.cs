using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace WhoIsWho.Api
{
    public class CountFunction
    {
        private readonly services.CognitiveSearchService searchService;

        public CountFunction(services.CognitiveSearchService s)
        {
            searchService = s;
        }

        [Function("Count")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
        {
            var json= await searchService.Count();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; odata.metadata=minimal");
            response.WriteString(json);

            return response;
        }
    }
}
