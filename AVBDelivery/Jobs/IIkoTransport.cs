using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using AVBDelivery.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using AVBDelivery.Models.Requests;
using AVBDelivery.Interfaces;
using AVBDelivery.Models.Responses;
using System.Text.Json.Serialization;
using NLog.Targets;
using System.Net.Http.Headers;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace AVBDelivery.Jobs
{
    public class IIkoTransport : IIikoTransport
    {
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        private IMemoryCache _memoryCache;
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://api-ru.iiko.services";
        private readonly AsyncPolicy _authRetryPolicy;
        private ILogger<IIkoTransport> _logger;
        private readonly JsonSerializerOptions jsonRequestSerializerOptions = new JsonSerializerOptions       
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private readonly JsonSerializerOptions jsonResponseSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private const string TOKENCACHEKEY = "token";
        public IIkoTransport(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _apiKey = configuration["TransportApi:ApiKey"];
            _authRetryPolicy = PollyPolicyFactory.CreateAuthRetryPolicy(GetAccessTokenAsync);
        }
        private string BuildEndpoint(string path) => $"{_apiUrl}{path}";
        public async Task<T> SendRequestAsync<T>(HttpRequestMessage request, string callerName)
        {
            return await _authRetryPolicy.ExecuteAsync(async () =>
            {
                if (!_memoryCache.TryGetValue(TOKENCACHEKEY, out string? accessToken))
                {
                    await GetAccessTokenAsync();
                    _memoryCache.TryGetValue(TOKENCACHEKEY, out accessToken);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseBody, jsonResponseSerializerOptions)!;
            });
        }

        public async Task<string> GetAccessTokenAsync()
        {
            string path = "/api/1/access_token";
            var requestData = new LoginRequestBody
            {
                ApiLogin = _apiKey,
            };
            var content = new StringContent(
                JsonSerializer.Serialize(requestData, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json");


            string finalUrl = BuildEndpoint(path);
            var response = await _httpClient.PostAsync(finalUrl, content);

            response.EnsureSuccessStatusCode();

            var responseStringBody = await response.Content.ReadAsStringAsync();
            LoginResponseBody responseBody = JsonSerializer.Deserialize<LoginResponseBody>(responseStringBody, jsonResponseSerializerOptions);
            
            _memoryCache.Set(TOKENCACHEKEY, responseBody.Token, TimeSpan.FromMinutes(55));

            return responseBody.Token;
        }

        public async Task<OrganizationsResponseBody> GetOrganizationsAsync()
        {
            string path = BuildEndpoint("/api/1/organizations");
            var content = new StringContent(
                JsonSerializer.Serialize("{}", jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
            var response = await SendRequestAsync<OrganizationsResponseBody>(request, "Get organizations");
            return response;
        }

        public async Task<ExternalMenusResponseBody> GetExternalMenusAsync()
        {
            string path = BuildEndpoint("/api/2/menu");
            var request = new HttpRequestMessage(HttpMethod.Post, path);
            string finalUrl = BuildEndpoint(path);
            var response = await SendRequestAsync<ExternalMenusResponseBody>(request, "Get external menus");
            return response;
        }

        public async Task<ExternalMenuByIdResponseBody> GetExternalMenuByIdAsync(string externalMenuId, string? priceCategoryId, string[] organizationIds)
        {
            string path = BuildEndpoint("/api/2/menu/by_id");
            var body = new ExternalMenuByIdRequestBody
            {
                ExternalMenuId = externalMenuId,
                OrganizationIds = organizationIds
            };
            if (priceCategoryId != null)
            {
                body.PriceCategoryId = priceCategoryId;
            }
            var content = new StringContent(
                JsonSerializer.Serialize(body, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
            var response = await SendRequestAsync<ExternalMenuByIdResponseBody>(request, "Get external menu by id");
            return response;
        }
        
    }
}
