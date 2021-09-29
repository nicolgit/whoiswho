using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;

namespace WhoIsWho.Portal.Api.Services
{
    public class CognitiveSearchService
    {
        private readonly ILogger logger;
		private readonly IConfiguration config;
        private readonly IHttpClientFactory clientFactory;
        private readonly string searchServiceKey;
        private readonly string searchServiceUrl;

        private readonly string urlSuggest;
        private readonly string urlResults;
        private readonly string urlResultCount;
        private readonly string indexName = "azuretable-index";

        public CognitiveSearchService (
			ILogger<CognitiveSearchService> loggerIn,
            IConfiguration configIn,
            IHttpClientFactory clientFactoryIn)
        {
            logger = loggerIn;
            config = configIn;
            clientFactory = clientFactoryIn;
            
            searchServiceKey = config["SEARCH_SERVICE_KEY"];
            searchServiceUrl = config["SEARCH_SERVICE_URL"];

            urlSuggest = searchServiceUrl +     "/indexes/" + indexName + "/docs/suggest?api-version=2019-05-06&suggesterName=default&highlightPreTag=<b>&highlightPostTag=</b>&$select=Type,Key,Name&fuzzy=true";
            urlResults = searchServiceUrl +     "/indexes/" + indexName + "/docs?api-version=2019-05-06";
            urlResultCount = searchServiceUrl + "/indexes/" + indexName + "/docs/$count?api-version=2019-05-06";
        }

        public async Task<string> Suggest(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,urlSuggest + "&$filter=ParentPartitionKey eq null&search=" + HttpUtility.UrlEncode(query));
            request.Headers.Add("api-key", searchServiceKey);
            request.Headers.Add("User-Agent", "Api");

            var client = clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<string> ResultByText(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,urlResults + "&$filter=ParentPartitionKey eq null&search=" + HttpUtility.UrlEncode(query));
            request.Headers.Add("api-key", searchServiceKey);
            request.Headers.Add("User-Agent", "Api");

            var client = clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<string> ResultByFilter(string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,urlResults + "&$filter=" + HttpUtility.UrlEncode(query));
            request.Headers.Add("api-key", searchServiceKey);
            request.Headers.Add("User-Agent", "Api");

            var client = clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }

        public async Task<string> Count()
        {
            var request = new HttpRequestMessage(HttpMethod.Get,urlResultCount);
            request.Headers.Add("api-key", searchServiceKey);
            request.Headers.Add("User-Agent", "Api");

            var client = clientFactory.CreateClient();

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return null;
            }
        }
    }
}