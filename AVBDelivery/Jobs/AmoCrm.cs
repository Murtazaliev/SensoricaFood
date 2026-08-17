using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using AVBDelivery.Models;
using AVBDelivery.Models.AmoCrm;
using AVBDelivery.Models.AmoCrm.Requests;
using AVBDelivery.Models.AmoCrm.Responses;
using Azure;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace AVBDelivery.Jobs
{
    public class AmoCrm
    {
        private HttpClient _httpClient;
        private string? _accessToken;
        private ApplicationContext _context;
        private string _apiUrl;
        ILogger<AmoCrm> _logger;

        public string? DriveUrl { get; set; }

        private readonly JsonSerializerOptions jsonResponseSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly JsonSerializerOptions jsonRequestSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        public AmoCrm(HttpClient httpClient, ApplicationContext context, IConfiguration configuration, ILogger<AmoCrm> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _apiUrl = string.Format("https://{0}.amocrm.ru", configuration["AmoCrmApi:Subdomain"]);
        }
        private async Task<string?> TryGetAccessToken()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            _accessToken = settings?.ApiKey;
            return _accessToken;
        }
        private async Task<T?> SendRequestAsync<T>(HttpRequestMessage request, string callerName)
        {
            var accessToken = _accessToken ?? await TryGetAccessToken();
            if (accessToken == null)
            {
                throw new Exception("Amo CRM пустой токен");
            }
            _logger.LogInformation($"{callerName}: {request.Content?.ReadAsStream()}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var response = await _httpClient.SendAsync(request);
            

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default(T);
            }

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"{callerName}: {responseBody.ToString()}");
                return JsonSerializer.Deserialize<T>(responseBody, jsonResponseSerializerOptions)!;
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"{callerName}: {responseBody.ToString()}");
                var serilizedError = JsonSerializer.Deserialize<ErrorBody>(responseBody, jsonResponseSerializerOptions)!;
                _logger.LogError($"{callerName} response error: {serilizedError.ToString()}");
            }
            return default(T);
        }

        public async Task<ProductSettingsResponseBody> GetProductSettings()
        {
            var endpoint = $"{_apiUrl}/api/v2/products_settings";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<ProductSettingsResponseBody>(request, "Get product settings");
            return response;
        }

        public async Task<GetCatalogsResponseBody> GetCatalogs()
        {
            var endpoint = $"{_apiUrl}/api/v4/catalogs";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetCatalogsResponseBody>(request, "Get catalogs");
            return response;
        }

        public async Task<GetCustomFieldsResponseBody> GetCustomFields(int? catalogId)
        {
            var endpoint = $"{_apiUrl}/api/v4/catalogs/{catalogId}/custom_fields";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetCustomFieldsResponseBody>(request, "Get custom fields");
            return response;
        }

        public async Task<GetElementsResponseBody?> GetElements(int? catalogId)
        {
            GetElementsResponseBody? response = null;
            GetElementsResponseBody? fullResponse = null;
            int page = 1;
            do
            {
                
                var endpoint = $"{_apiUrl}/api/v4/catalogs/{catalogId}/elements?page={page}";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                response = await SendRequestAsync<GetElementsResponseBody>(request, "Get elements");
                if (response != null)
                {
                    if (fullResponse == null)
                    {
                        fullResponse = response;
                    }
                    else
                    {
                        var elementsList = new List<Element>();
                        elementsList.AddRange(fullResponse.Embedded.Elements);
                        elementsList.AddRange(response.Embedded.Elements);
                        fullResponse.Embedded.Elements = elementsList.ToArray();
                    }
                }
                page++;
            }
            while (response != null);
            
            return fullResponse;
        }
        
        public async Task<CreateElementsResponseBody> CreateElements(List<Element> elementsToCreate, int? catalogId)
        {
            var endpoint = $"{_apiUrl}/api/v4/catalogs/{catalogId}/elements";
            var content = new StringContent(
                JsonSerializer.Serialize(elementsToCreate, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateElementsResponseBody>(request, "Create elements");
            return response;
        }

        public async Task<CreateElementsResponseBody> UpdateElements(List<Element> elementsToUpdate, int? catalogId)
        {
            var endpoint = $"{_apiUrl}/api/v4/catalogs/{catalogId}/elements";
            var content = new StringContent(
                JsonSerializer.Serialize(elementsToUpdate, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateElementsResponseBody>(request, "Update elements");
            return response;
        }


        #region Contacts
        public async Task<AmoContact> GetContact(string contactId)
        {
            var endpoint = $"{_apiUrl}/api/v4/contacts/{contactId}";
            var singleRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<AmoContact>(singleRequest, "Get contact");
            return response;
        }
        public async Task<GetContactsResponseBody> GetContacts()
        {
            GetContactsResponseBody? response = null;
            GetContactsResponseBody? fullResponse = null;

            int page = 1;
            do
            {
                var requestEndPoint = $"{_apiUrl}/api/v4/contacts?page={page}";
                var request = new HttpRequestMessage(HttpMethod.Get, requestEndPoint);
                response = await SendRequestAsync<GetContactsResponseBody>(request, "Get contacts");
                if (response != null)
                {
                    if (fullResponse == null)
                    {
                        fullResponse = response;
                    }
                    else
                    {
                        var elementsList = new List<AmoContact>();
                        elementsList.AddRange(fullResponse.Embedded.Contacts);
                        elementsList.AddRange(response.Embedded.Contacts);
                        fullResponse.Embedded.Contacts = elementsList.ToArray();
                    }
                }
                page++;
            }
            while (response != null);

            return fullResponse;


        }      
        public async Task<GetContactsCustomFieldsResponseBody> GetContactsCustomFieldsAsync()
        {
            var endpoint = $"{_apiUrl}/api/v4/contacts/custom_fields";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetContactsCustomFieldsResponseBody>(request, "Get contacts custom fields");
            return response;
        }
        public async Task<CreateContactsResponseBody> UpdateContacts(IEnumerable<AmoContact> contacts)
        {
            var endpoint = $"{_apiUrl}/api/v4/contacts";
            var content = new StringContent(
                JsonSerializer.Serialize(contacts, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateContactsResponseBody>(request, "Update contacts");
            return response;
        }
        public async Task<CreateContactsResponseBody> CreateContacts(IEnumerable<AmoContact> contacts)
        {
            var endpoint = $"{_apiUrl}/api/v4/contacts";
            var content = new StringContent(
                JsonSerializer.Serialize(contacts, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateContactsResponseBody>(request, "Create contacts");
            return response;
        }
        #endregion


        #region Companies
        public async Task<GetCustomFieldsResponseBody> GetCompaniesCustomFields()
        {
            var endpoint = $"{_apiUrl}/api/v4/companies/custom_fields";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetCustomFieldsResponseBody>(request, "Get companies custom fields");
            return response;
        }
        public async Task<GetCompaniesResponseBody> GetCompaniesAsync()
        {
            var endpoint = $"{_apiUrl}/api/v4/companies";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetCompaniesResponseBody>(request, "Get companies");
            return response;
        }
        public async Task<CreateCompaniesResponseBody> CreateCompanies(IEnumerable<Company> companies)
        {
            var endpoint = $"{_apiUrl}/api/v4/companies";
            var content = new StringContent(
                JsonSerializer.Serialize(companies, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateCompaniesResponseBody>(request, "Create companies");
            return response;
        }
        public async Task<CreateCompaniesResponseBody> UpdateCompanies(IEnumerable<Company> companies)
        {
            var endpoint = $"{_apiUrl}/api/v4/companies";
            var content = new StringContent(
                JsonSerializer.Serialize(companies, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateCompaniesResponseBody>(request, "Update companies");
            return response;
        }
        #endregion



        #region Leads
        public async Task<GetCustomFieldsResponseBody> GetLeadsCustomFields()
        {
            var endpoint = $"{_apiUrl}/api/v4/leads/custom_fields";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetCustomFieldsResponseBody>(request, "Get leads custom fields");
            return response;
        }

        public async Task<GetLeadResponseBody> GetLead(int id)
        {
            var endpoint = $"{_apiUrl}/api/v4/leads/{id}";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetLeadResponseBody>(request, "Get lead by id");
            return response;
        }

        public async Task<CreateLeadResponseBody> CreateLeads(IEnumerable<Lead> leads)
        {
            var endpoint = $"{_apiUrl}/api/v4/leads";
            var content = new StringContent(
                JsonSerializer.Serialize(leads, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateLeadResponseBody > (request, "Create lead");
            return response;
        }

        public async Task<CreateLeadResponseBody> UpdateLeads(IEnumerable<Lead> leads)
        {
            var endpoint = $"{_apiUrl}/api/v4/leads";
            var content = new StringContent(
                JsonSerializer.Serialize(leads, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateLeadResponseBody>(request, "Create lead");
            return response;
        }

        public async Task<GetLeadLinksResponseBody> GetLeadLinks(int leadId)
        {
            var endpoint = $"{_apiUrl}/api/v4/leads/{leadId}/links";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetLeadLinksResponseBody>(request, "Get leads links");
            return response;
        }
        public async Task<CreateLeadLinkResponseBody> CreateLeadLink(IEnumerable<Link> links, int leadId)
        {
            var endpoint = $"{_apiUrl}/api/v4/leads/{leadId}/link";
            var content = new StringContent(
                JsonSerializer.Serialize(links, jsonRequestSerializerOptions),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
            var response = await SendRequestAsync<CreateLeadLinkResponseBody>(request, "Create lead link");
            return response;
        }
        public async Task<GetLeadPipelinesResponseBody> GetLeadPipelines()
        {
            var endpoint = $"{_apiUrl}/api/v4/leads/pipelines";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetLeadPipelinesResponseBody>(request, "Get leads pipelines");
            return response;
        }
        #endregion
        public async Task<GetAccountInfoResponseBody?> GetAccountInfo()
        {
            var endpoint = $"{_apiUrl}/api/v4/account?with=drive_url";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetAccountInfoResponseBody>(request, "Get account info");
            return response;
        }

        public async Task<GetFileByUuidResponseBody?> GetFileByUuid(string uuid)
        {
            var endpoint = $"{DriveUrl}/v1.0/files/{uuid}";
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var response = await SendRequestAsync<GetFileByUuidResponseBody>(request, "Get file by uuid");
            return response;
        }
    }
}
