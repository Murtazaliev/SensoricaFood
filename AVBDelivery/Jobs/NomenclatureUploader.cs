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

                var products = await _context.Products.ToListAsync();
                var group = await _context.ProductGroups.AsNoTracking().ToListAsync();
                var nomenclature = await _context.Nomenclature.FirstOrDefaultAsync();

                if (!(group == nomenclature?.ProductGroup && products == nomenclature?.Products))
                {
                    if (products != null)
                    {
                        _logger.LogInformation($"Удаляем продукты. Их {products.Count}.");
                        _context.Products.RemoveRange(products);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Продукты удалены.");
                    }

                    group = await _context.ProductGroups.Include(x => x.Products).ToListAsync();
                    if (group != null)
                    {
                        _logger.LogInformation($"Удаляем группы. Их {group.Count}");
                        foreach (var item in group)
                        {
                            if (item.Products.Count == 0)
                            {
                                _logger.LogInformation($"Удаляем группу \"{item.GroupName}\", т.к. в ней нет ручной номенклатуры.");
                                _context.ProductGroups.RemoveRange(item);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }

                string defaultGroupId = Guid.NewGuid().ToString();
                var defaultGroup = new ProductGroup
                {
                    GroupName = "Товары",
                    Id = defaultGroupId,
                    Products = new List<Product>()
                };

                await _context.ProductGroups.AddAsync(defaultGroup);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Создана группа по умолчанию");

                _logger.LogInformation("Получаем настройки аккаунт");
                var accountInfo = await _amoCrm.GetAccountInfo();
                _amoCrm.DriveUrl = accountInfo?.DriveUrl;

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                };

                foreach (var product in amoCrmProducts)
                {
                    var amoProductGroup = product.CustomFieldsValues?.FirstOrDefault(g => g.FieldCode == "GROUP");
                    var amoProductGroupEnumId = amoProductGroup?.Values?.FirstOrDefault()?.EnumId;
                    var groupId = amoProductGroupEnumId == null ? defaultGroupId : amoProductGroupEnumId.ToString();

                    var dbGroup = await _context.ProductGroups.FirstOrDefaultAsync(g => g.Id == groupId);
                    if (dbGroup == null)
                    {
                        dbGroup = new ProductGroup
                        {
                            Id = groupId,
                            GroupName = (amoProductGroup?.Values?.FirstOrDefault()?.Value)?.ToString() ?? "Группа",
                            Products = new List<Product>()
                        };
                        await _context.ProductGroups.AddAsync(dbGroup);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Создана группа {dbGroup.GroupName}");
                    }

                    var customFieldValues = product.CustomFieldsValues; // CustomFieldValues[] ?

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

                    // --------- MULTI-IMAGE: все file-поля и все values ---------
                    var productId = product.Id.ToString();

                    var fileFields = (customFieldValues ?? Array.Empty<CustomFieldValues>())
                        .Where(f => f.FieldType == "file")
                        .OrderByDescending(f => f.FieldCode == "IMAGE") // IMAGE первым
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

                    // Главная (первая) — опционально пишем в БД для совместимости
                    byte[]? mainImage = null;
                    if (indexes.Count > 0)
                        mainImage = await _distributedCache.GetAsync($"prod:{productId}:img:{indexes[0]}");
                    // ----------------------------------------------------------

                    var productToCreate = new Product
                    {
                        AmoCrmId = product.Id,
                        Description = description,
                        Id = productId,
                        ParentGroupName = dbGroup.GroupName,
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

                    dbGroup.Products.Add(productToCreate);
                    _logger.LogInformation($"Добавлен товар {productToCreate.Name}");
                }

                var defGroup = await _context.ProductGroups.FirstOrDefaultAsync(g => g.Id == defaultGroupId);
                if (defGroup != null && defGroup.Products.Count == 0)
                {
                    _context.ProductGroups.Remove(defGroup);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Обмен завершен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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