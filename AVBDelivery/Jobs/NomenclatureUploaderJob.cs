using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NLog;
using Quartz;

namespace AVBDelivery.Jobs
{
    [DisallowConcurrentExecution]
    public class NomenclatureUploaderJob : IJob
    {
        private IServiceProvider _serviceProvider;
        private ILogger<NomenclatureUploaderJob> _logger;
        private HttpClient _httpClient;
        private IConfiguration _configuration;
        public NomenclatureUploaderJob(IServiceProvider serviceProvider, HttpClient httpClient, ILogger<NomenclatureUploaderJob> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.MergedJobDataMap;
                string apiKey;
                var apiKeyResult = dataMap.TryGetString("apiKey", out apiKey);
                if (!apiKeyResult || apiKey.IsNullOrEmpty())
                {
                    _logger.LogError("Пустой api ключ");
                    return;
                }
                var nomenclatureUploader = _serviceProvider.GetService<INomenclatureUploader>();
                await nomenclatureUploader.Start();
                //var nomenclatureUploader = _serviceProvider.GetService<INomenclatureUploader>();
                ////var accessToken = await nomenclatureUploader.GetAccessTokenAsync();
                //var organizationsRequest = await nomenclatureUploader.GetOrganizationsAsync();
                //var organization = organizationsRequest.Organizations.FirstOrDefault();
                //var externalMenusRequest = await nomenclatureUploader.GetExternalMenusAsync();
                //var externalMenu = externalMenusRequest.ExternalMenus.FirstOrDefault();
                //var priceCategory = externalMenusRequest.PriceCategories.FirstOrDefault();
                //var externalMenuByIdRequest = await nomenclatureUploader.GetExternalMenuByIdAsync(externalMenu.Id, priceCategory.Id, [organization.Id]);
                //var externalMenuById = externalMenuByIdRequest

                //_logger.LogInformation($"Работает {externalMenu.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
