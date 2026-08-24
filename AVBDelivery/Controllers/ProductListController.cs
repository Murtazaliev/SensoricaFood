using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace AVBDelivery.Controllers
{
    public class ProductListController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IDistributedCache _distributedCache;

        private User? _User;
        private IList<string>? _Roles;

        public ProductListController(
            ApplicationContext context,
            UserManager<User> userManager,
            IDistributedCache distributedCache)
        {
            _context = context;
            _userManager = userManager;
            _distributedCache = distributedCache;
        }

        // GET: ProductList
        [Authorize]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> Index()
        {
            return View(await GetMenu());
        }

        private async Task<ProductListViewModel> GetMenu()
        {
            var nom = new Nomenclature
            {
                ProductGroup = await _context.ProductGroups
                    .Select(g => new ProductGroup
                    {
                        Id = g.Id,
                        GroupName = g.GroupName,
                        Products = g.Products.Select(p => new Product
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Price = p.Price,
                            Type = p.Type,
                            IsActive = p.IsActive,
                            ParentGroupName = p.ParentGroupName,
                            ProductInBlackList = p.ProductInBlackList,
                            MeasureUnit = p.MeasureUnit,
                            Sku = p.Sku,
                            Description = p.Description,
                            AmoCrmId = p.AmoCrmId,
                            FullEnergy = p.FullEnergy,
                            PortionGram = p.PortionGram
                        }).ToList()
                    })
                    .ToListAsync()
            };

            var announcement = await _context.SiteAnnouncements.FirstOrDefaultAsync();

            await GetUserInfo();

            // Фильтрация по персональному меню организации клиента.
            // null = показывать всё (обратная совместимость).
            var productFilter = await GetUserMenuFilter();
            if (productFilter != null)
            {
                foreach (var g in nom.ProductGroup)
                {
                    g.Products = g.Products.Where(p => productFilter.Contains(p.Id)).ToList();
                }
                nom.ProductGroup.RemoveAll(g => g.Products.Count == 0);
            }

            var order = new Order { Items = new() };
            if (_User != null)
            {
                var userId = _User.Id;
                var today = DateTime.UtcNow.Date;
                var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

                var cachedJson = await _distributedCache.GetStringAsync(cacheKey);
                var shoppingCart = !string.IsNullOrEmpty(cachedJson)
                    ? (JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson) ?? new List<ShoppingCart>())
                    : new List<ShoppingCart>();

                foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
                {
                    var first = item.FirstOrDefault();
                    if (first == null) continue;

                    var orderItem = new OrderItem
                    {
                        Count = item.Sum(x => x.Count),
                        Price = first.Price,
                        ProductName = first.ProductName,
                        ProductId = first.ProductId,
                        MeasureUnit = first.MeasureUnit
                    };

                    if (orderItem.Count > 0)
                    {
                        order.Items.Add(orderItem);
                        order.Sum += orderItem.Price * orderItem.Count;
                    }
                }
            }

            // NEW: собираем индексы картинок для каруселей (prodId -> [0,1,2...])
            // Требуется, чтобы в ProductListViewModel было поле:
            // public Dictionary<string, List<int>> ProductImageIndexes { get; set; } = new();
            var productIds = nom.ProductGroup
                .SelectMany(g => g.Products)
                .Select(p => p.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var map = new Dictionary<string, List<int>>(productIds.Count);
            foreach (var pid in productIds)
            {
                map[pid] = await GetImageIndexesAsync(pid);
            }

            return new ProductListViewModel
            {
                NomenclatureList = nom,
                Order = order,
                SiteAnnouncement = announcement,
                ProductImageIndexes = map
            };
        }

        private async Task<List<string>?> GetUserMenuFilter()
        {
            if (_User == null)
            {
                return null;
            }

            var contact = await _context.Contacts
                .Include(c => c.Organizations)
                .FirstOrDefaultAsync(c => c.UserId == _User.Id);

            if (contact?.Organizations == null)
            {
                return null;
            }

            var menuIds = contact.Organizations
                .Where(o => o.MenuId != null)
                .Select(o => o.MenuId!.Value)
                .Distinct()
                .ToList();

            // Все организации без персонального меню (или их нет) — показываем всё.
            if (menuIds.Count == 0)
            {
                return null;
            }

            return await _context.MenuProducts
                .Where(mp => menuIds.Contains(mp.MenuId))
                .Select(mp => mp.ProductId)
                .Distinct()
                .ToListAsync();
        }

        private async Task<List<int>> GetImageIndexesAsync(string productId)
        {
            var metaKey = $"prod:{productId}:imgs";
            var json = await _distributedCache.GetStringAsync(metaKey);

            // если мета нет — считаем, что есть только "0" (совместимость)
            if (string.IsNullOrWhiteSpace(json))
                return new List<int> { 0 };

            var list = JsonSerializer.Deserialize<List<int>>(json);
            return (list == null || list.Count == 0) ? new List<int> { 0 } : list;
        }

        // GET: /ProductList/RenderImage?id=2565545&n=0
        [HttpGet]
        [AllowAnonymous] // если меню доступно только авторизованным, можно убрать
        public async Task<ActionResult> RenderImage(string id, int n = 0)
        {
            // 1) Redis (distributed cache)
            var imgKey = $"prod:{id}:img:{n}";
            var bytes = await _distributedCache.GetAsync(imgKey);

            if (bytes != null && bytes.Length > 0)
            {
                Response.Headers["Cache-Control"] = "public, max-age=3600";
                return File(bytes, "image/png");
            }

            // 2) fallback: старая схема (одна картинка в БД)
            if (n == 0)
            {
                var productItem = await _context.Products.FindAsync(id);
                if (productItem?.Image != null && productItem.Image.Length > 0)
                {
                    Response.Headers["Cache-Control"] = "public, max-age=3600";
                    return File(productItem.Image, "image/png");
                }
            }

            return NotFound();
        }

        private async Task GetUserInfo()
        {
            try
            {
                var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaim != null)
                {
                    _User = await _userManager.FindByIdAsync(userClaim.Value);
                }

                if (_User != null)
                {
                    _Roles = await _userManager.GetRolesAsync(_User);
                }
            }
            catch (Exception ex)
            {
                await DBConnector.DBLogs.Error(
                    "Не удалось получить информацию по пользователю",
                    ClaimTypes.NameIdentifier,
                    $"{ex.Message}\n{ex.InnerException}"
                );
            }
        }
    }
}