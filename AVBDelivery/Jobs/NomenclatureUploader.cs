using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using AVBDelivery.Models.AmoCrm;
using AVBDelivery.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace AVBDelivery.Jobs
{
    public class NomenclatureUploader : INomenclatureUploader
    {
        private readonly IIikoTransport _iikoTransport;
        private readonly AmoCrm _amoCrm;
        private readonly ILogger<NomenclatureUploader> _logger;
        private readonly ApplicationContext _context;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _distributedCache;

        public NomenclatureUploader(
            IIikoTransport iikoTransport,
            AmoCrm amoCrm,
            ApplicationContext context,
            HttpClient httpClient,
            IDistributedCache distributedCache,
            ILogger<NomenclatureUploader> logger)
        {
            _iikoTransport = iikoTransport;
            _logger = logger;
            _context = context;
            _httpClient = httpClient;
            _amoCrm = amoCrm;
            _distributedCache = distributedCache;
        }

        public async Task<string> Start()
        {
            try
            {
                _logger.LogInformation("Получение примечаний");
                var fields = await _amoCrm.GetLeadsCustomFields();
                var noteField = fields.Embedded.CustomFields.FirstOrDefault(f => f.Id == 979843);
                if (noteField != null)
                {
                    foreach (var field in noteField.Enums)
                    {
                        var existed = await _context.Notes.FirstOrDefaultAsync(n => n.AmoCrmId == field.Id);
                        if (existed != null)
                        {
                            existed.Value = field.Value;
                            existed.IsDeleted = false;
                        }
                        else
                        {
                            await _context.Notes.AddAsync(new Note
                            {
                                AmoCrmId = field.Id,
                                Value = field.Value,
                                IsDeleted = false
                            });
                        }
                    }
                }
                _logger.LogInformation("Примечания получены");

                _logger.LogInformation("Запущен обмен номенклатуры");
                _logger.LogInformation("Получаем списки в AmoCrm");

                var catalogs = await _amoCrm.GetCatalogs();
                var productCatalogId = catalogs.Embedded.Catalogs?.FirstOrDefault(c => c.Type == "products")?.Id;
                if (productCatalogId == null)
                {
                    _logger.LogError("Отсутствует список товаров в AmoCrm");
                    throw new Exception("Отсутствует список товаров в AmoCrm");
                }

                _logger.LogInformation("Получаем поля в AmoCrm");
                _ = await _amoCrm.GetCustomFields(productCatalogId);

                _logger.LogInformation("Получаем товары в AmoCrm");
                var amoCrmProductsResponse = await _amoCrm.GetElements(productCatalogId);
                var amoCrmProducts = amoCrmProductsResponse?.Embedded.Elements;
                if (amoCrmProducts == null)
                {
                    _logger.LogError("Отсутствует список товаров в AmoCrm");
                    throw new Exception("Отсутствует список товаров в AmoCrm");
                }

                _logger.LogInformation("Получаем настройки аккаунта");
                var accountInfo = await _amoCrm.GetAccountInfo();
                _amoCrm.DriveUrl = accountInfo?.DriveUrl;

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };

                var stagingGroups = new Dictionary<string, ProductGroup>();
                var stagingProducts = new List<Product>();
                string defaultGroupId = Guid.NewGuid().ToString();

                foreach (var product in amoCrmProducts)
                {
                    var amoProductGroup = product.CustomFieldsValues?.FirstOrDefault(g => g.FieldCode == "GROUP");
                    var amoProductGroupEnumId = amoProductGroup?.Values?.FirstOrDefault()?.EnumId;
                    var groupId = amoProductGroupEnumId == null ? defaultGroupId : amoProductGroupEnumId.ToString();

                    if (!stagingGroups.ContainsKey(groupId))
                    {
                        stagingGroups[groupId] = new ProductGroup
                        {
                            Id = groupId,
                            GroupName = (amoProductGroup?.Values?.FirstOrDefault()?.Value)?.ToString() ?? "Группа",
                            Products = new List<Product>()
                        };
                    }

                    var customFieldValues = product.CustomFieldsValues;

                    var descriptionFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldCode == "DESCRIPTION")?.Values?.FirstOrDefault()?.Value;
                    var description = descriptionFieldValue?.ToString() ?? "";

                    var priceFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldCode == "PRICE")?.Values?.FirstOrDefault()?.Value;
                    var price = priceFieldValue == null ? 0 : float.Parse(priceFieldValue.ToString());

                    var measureUnitFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldCode == "UNIT")?.Values?.FirstOrDefault()?.Value;
                    var measureUnit = measureUnitFieldValue?.ToString() ?? "";

                    var energyFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldName == "КБЖУ")?.Values?.FirstOrDefault()?.Value;
                    var energyAmount = energyFieldValue?.ToString() ?? "";

                    var weightFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldName == "Вес")?.Values?.FirstOrDefault()?.Value;
                    var weight = weightFieldValue == null ? 0 : float.Parse(weightFieldValue.ToString());

                    var skuFieldValue = customFieldValues?.FirstOrDefault(f => f.FieldCode == "SKU")?.Values?.FirstOrDefault()?.Value;
                    var sku = skuFieldValue?.ToString() ?? "";

                    var productId = product.Id.ToString();

                    var fileFields = (customFieldValues ?? Array.Empty<CustomFieldValues>())
                        .Where(f => f.FieldType == "file")
                        .OrderByDescending(f => f.FieldCode == "IMAGE")
                        .ToList();

                    var seenFileUuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var indexes = new List<int>();
                    int n = 0;

                    foreach (var fileField in fileFields)
                    {
                        foreach (var vv in fileField.Values ?? Array.Empty<ElementValue>())
                        {
                            if (vv?.Value == null) continue;

                            var fv = TryGetFileValue(vv.Value);
                            if (fv == null) continue;

                            if (fv.IsDeleted == true) continue;
                            if (string.IsNullOrWhiteSpace(fv.FileUuid)) continue;
                            if (!seenFileUuids.Add(fv.FileUuid)) continue;

                            var file = await _amoCrm.GetFileByUuid(fv.FileUuid);
                            var href = file?.Links?.Download?.Href;
                            if (string.IsNullOrWhiteSpace(href)) continue;

                            var img = await DownloadImageAsync(new Uri(href));
                            if (img == null || img.Length == 0) continue;

                            await _distributedCache.SetAsync($"prod:{productId}:img:{n}", img, cacheOptions);
                            indexes.Add(n);
                            n++;
                        }
                    }

                    await _distributedCache.SetStringAsync(
                        $"prod:{productId}:imgs",
                        JsonSerializer.Serialize(indexes),
                        cacheOptions
                    );

                    byte[]? mainImage = null;
                    if (indexes.Count > 0)
                        mainImage = await _distributedCache.GetAsync($"prod:{productId}:img:{indexes[0]}");

                    var productToCreate = new Product
                    {
                        AmoCrmId = product.Id,
                        Description = description,
                        Id = productId,
                        ParentGroupName = stagingGroups[groupId].GroupName,
                        Price = price,
                        MeasureUnit = measureUnit,
                        Name = product.Name,
                        IsActive = true,
                        FullEnergy = energyAmount,
                        ProductInBlackList = false,
                        PortionGram = weight,
                        Sku = sku,
                        Type = 1,
                        Image = mainImage
                    };

                    stagingGroups[groupId].Products.Add(productToCreate);
                    stagingProducts.Add(productToCreate);
                    _logger.LogInformation($"Загружен в staging: {productToCreate.Name}");
                }

                _logger.LogInformation($"Из AmoCRM загружено {stagingProducts.Count} товаров в {stagingGroups.Count} групп. Начинаем обновление БД.");

                var existingProducts = await _context.Products.ToListAsync();
                _context.Products.RemoveRange(existingProducts);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Старые продукты удалены из БД.");

                var existingGroups = await _context.ProductGroups.Include(x => x.Products).ToListAsync();
                foreach (var item in existingGroups)
                {
                    if (item.Products.Count == 0)
                    {
                        _context.ProductGroups.Remove(item);
                    }
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Пустые группы удалены из БД.");

                foreach (var groupPair in stagingGroups)
                {
                    await _context.ProductGroups.AddAsync(groupPair.Value);
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Новые группы добавлены в БД.");

                await _context.SaveChangesAsync();
                _logger.LogInformation("Обмен завершен успешно.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обмене номенклатуры. БД осталась без изменений.");
                return "done";
            }

            return "done";
        }

        private static FileValue? TryGetFileValue(object value)
        {
            try
            {
                // vv.Value может быть JsonElement (самый частый кейс при object)
                if (value is JsonElement je)
                    return je.Deserialize<FileValue>();

                // fallback: сериализуем как есть
                var json = JsonSerializer.Serialize(value);
                return JsonSerializer.Deserialize<FileValue>(json);
            }
            catch
            {
                return null;
            }
        }

        private async Task<byte[]?> DownloadImageAsync(Uri uri)
        {
            try
            {
                var imageArray = await _httpClient.GetByteArrayAsync(uri);

                using (Image img = Image.Load(imageArray))
                {
                    img.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Crop,
                        Size = new SixLabors.ImageSharp.Size(640, 480)
                    }));

                    using var ms = new MemoryStream();
                    img.Save(ms, new PngEncoder());
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return null;
            }
        }
    }
}