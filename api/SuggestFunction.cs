using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace WhoIsWho.Api
{
    public class SuggestFunction
    {
        private readonly services.CognitiveSearchService searchService;

        public SuggestFunction(services.CognitiveSearchService s)
        {
            searchService = s;
        }

        [Function("Suggest")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req, FunctionContext executionContext)
        {
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var searchString = queryDictionary["search"];

            var json= await searchService.Suggest(searchString);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; odata.metadata=minimal");
            response.WriteString(json);

            return response;
        }
    }
}