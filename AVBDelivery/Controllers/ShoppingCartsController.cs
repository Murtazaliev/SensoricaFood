using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace AVBDelivery.Controllers
{

    /// <summary>
    /// ShoppingCardItems!!
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        User _User;
        IList<string> _Roles;

        public ShoppingCartsController(ApplicationContext context, IConnectionMultiplexer redis, UserManager<User> userManager, IDistributedCache cache)
        {
            _redis = redis;
            _db = redis.GetDatabase();
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<List<ShoppingCart>>> GetShoppingCart()
        {
            await GetUserInfo();

            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{_User.Id}:{today:yyyy-MM-dd}";

            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(cachedJson))
                return new List<ShoppingCart>(); // Возвращаем пустую корзину

            var cartItems = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);
            return cartItems ?? new List<ShoppingCart>();
        }

        //// GET: api/ShoppingCarts/5
        //[HttpGet("{id}")]
        //[Authorize(Roles = "client")]
        //public async Task<ActionResult<ShoppingCart>> GetShoppingCart(int id)
        //{
        //    var shoppingCart = await _context.ShoppingCart.FindAsync(id);

        //    if (shoppingCart == null)
        //    {
        //        return NotFound();
        //    }

        //    return shoppingCart;
        //}

        // PUT: api/ShoppingCarts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> PutShoppingCart(int id, ShoppingCart shoppingCart)
        //{
        //    if (id != shoppingCart.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(shoppingCart).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ShoppingCartExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //[HttpPost]
        //[Authorize(Roles = "client")]
        //public async Task<ActionResult<ShoppingCart>> PostShoppingCarts(ShoppingCart shoppingCart)
        //{
        //    await GetUserInfo();
        //    var userId = _User.Id;
        //    var today = DateTime.UtcNow.Date;
        //    var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

        //    var product = await _context.Products
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.Id == shoppingCart.ProductId);

        //    if (product == null)
        //        return BadRequest("Товар не найден");

        //    bool saved = false;
        //    ShoppingCart updatedItem = null;

        //    while (!saved)
        //    {
        //        // 1. GET текущая корзина и фиксируем её значение
        //        var originalValue = await _db.StringGetAsync(cacheKey);
        //        List<ShoppingCart> cart = !originalValue.IsNullOrEmpty
        //            ? JsonSerializer.Deserialize<List<ShoppingCart>>(originalValue)
        //            : new List<ShoppingCart>();

        //        // 2. Применяем изменения в модели
        //        var existingItem = cart.FirstOrDefault(x => x.ProductId == shoppingCart.ProductId);

        //        if (existingItem != null)
        //        {
        //            existingItem.Count += shoppingCart.Count;

        //            if (existingItem.Count <= 0)
        //            {
        //                cart.Remove(existingItem);
        //                updatedItem = null;
        //            }
        //            else
        //            {
        //                updatedItem = existingItem;
        //            }
        //        }
        //        else
        //        {
        //            if (shoppingCart.Count <= 0)
        //                return BadRequest("Количество должно быть > 0");

        //            var newItem = new ShoppingCart
        //            {
        //                ProductId = product.Id,
        //                ProductName = product.Name,
        //                Count = shoppingCart.Count,
        //                Price = product.Price,
        //                UserId = userId,
        //                OrderDate = today,
        //                MeasureUnit = product.MeasureUnit,
        //                AmoCrmId = product.AmoCrmId
        //            };

        //            cart.Add(newItem);
        //            updatedItem = newItem;
        //        }

        //        // 3. Если корзина опустела → удалить ключ атомарно
        //        if (cart.Count == 0)
        //        {
        //            var tranDelete = _db.CreateTransaction();
        //            tranDelete.AddCondition(
        //                originalValue.IsNullOrEmpty
        //                    ? Condition.KeyNotExists(cacheKey)
        //                    : Condition.StringEqual(cacheKey, originalValue)
        //            );

        //            _ = tranDelete.KeyDeleteAsync(cacheKey);
        //            saved = await tranDelete.ExecuteAsync();

        //            if (saved)
        //                return Ok(null);

        //            continue;
        //        }

        //        // 4. Serialize new cart JSON
        //        var newJson = JsonSerializer.Serialize(cart);

        //        // 5. Создаём транзакцию с условием
        //        var tran = _db.CreateTransaction();

        //        tran.AddCondition(
        //            originalValue.IsNullOrEmpty
        //                ? Condition.KeyNotExists(cacheKey)
        //                : Condition.StringEqual(cacheKey, originalValue)
        //        );

        //        _ = tran.StringSetAsync(cacheKey, newJson, TimeSpan.FromHours(2));

        //        saved = await tran.ExecuteAsync();
        //    }

        //    return Ok(updatedItem);
        //}

        [HttpPost]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<List<ShoppingCart>>> PostShoppingCarts(ShoppingCart shoppingCart)
        {
            await GetUserInfo();
            var userId = _User.Id;
            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

            // Загружаем товар
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == shoppingCart.ProductId);

            if (product == null)
                return BadRequest("Товар не найден");

            // Загружаем корзину из Redis
            var cachedJson = await _cache.GetStringAsync(cacheKey);
            List<ShoppingCart> cart;

            if (!string.IsNullOrEmpty(cachedJson))
            {
                cart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);
            }
            else
            {
                cart = new List<ShoppingCart>();
            }

            // Обновляем корзину
            var existingItem = cart.FirstOrDefault(x => x.ProductId == shoppingCart.ProductId);
            if (existingItem != null)
            {
                existingItem.Count += shoppingCart.Count;
                if (existingItem.Count <= 0)
                    cart.Remove(existingItem);
            }
            else
            {
                if (shoppingCart.Count <= 0)
                    return BadRequest("Нельзя добавить 0 или отрицательное количество");

                cart.Add(new ShoppingCart
                {
                    ProductId = shoppingCart.ProductId,
                    Count = shoppingCart.Count,
                    UserId = userId,
                    OrderDate = today,
                    Price = product.Price,
                    ProductName = product.Name,
                    MeasureUnit = product.MeasureUnit,
                    AmoCrmId = product.AmoCrmId
                });
            }

            // Удаляем корзину, если пуста
            if (cart.Count == 0)
            {
                await _cache.RemoveAsync(cacheKey);
                return new List<ShoppingCart>();
            }

            // Сохраняем обратно в Redis
            var newJson = JsonSerializer.Serialize(cart);
            await _cache.SetStringAsync(cacheKey, newJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            });

            return cart;
        }

        [HttpPost("Update")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<List<ShoppingCart>>> UpdateShoppingCarts(List<ShoppingCart> shoppingCarts)
        {
            await GetUserInfo();
            var userId = _User.Id;
            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

            // Загружаем товар
            var products = await _context.Products
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id);

            List<ShoppingCart> cart = new List<ShoppingCart>();
            foreach (var shoppingCart in shoppingCarts)
            {
                if (!products.TryGetValue(shoppingCart.ProductId, out var product))
                    continue;

                if (shoppingCart.Count <= 0)
                    continue;

                cart.Add(new ShoppingCart
                {
                    ProductId = shoppingCart.ProductId,
                    Count = shoppingCart.Count,
                    UserId = userId,
                    OrderDate = today,
                    Price = product.Price,
                    ProductName = product.Name,
                    MeasureUnit = product.MeasureUnit,
                    AmoCrmId = product.AmoCrmId
                });
            }

            if (cart.Count == 0)
            {
                await _cache.RemoveAsync(cacheKey);
                return new List<ShoppingCart>();
            }

            var newJson = JsonSerializer.Serialize(cart);
            await _cache.SetStringAsync(cacheKey, newJson, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            });

            return cart;
        }

        //[HttpPost]
        //[Authorize(Roles = "client")]
        //public async Task<ActionResult<List<ShoppingCart>>> PostShoppingCart(ShoppingCart shoppingCart)
        //{
        //    await GetUserInfo();

        //    var product = await _context.Products
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.Id == shoppingCart.ProductId);

        //    if (product == null)
        //        return BadRequest("Товар не найден");

        //    var today = DateTime.UtcNow.Date;

        //    // Находим существующую позицию в корзине на сегодня
        //    var existingItem = await _context.ShoppingCart.FirstOrDefaultAsync(x =>
        //        x.UserId == _User.Id &&
        //        x.ProductId == shoppingCart.ProductId &&
        //        x.OrderDate.Date == today);

        //    if (existingItem != null)
        //    {
        //        existingItem.Count += shoppingCart.Count;

        //        if (existingItem.Count <= 0)
        //        {
        //            _context.ShoppingCart.Remove(existingItem);
        //        }
        //    }
        //    else
        //    {
        //        if (shoppingCart.Count <= 0)
        //            return BadRequest("Нельзя добавить 0 или отрицательное количество");

        //        shoppingCart.UserId = _User.Id;
        //        shoppingCart.OrderDate = today;
        //        shoppingCart.Price = product.Price;
        //        shoppingCart.ProductName = product.Name;
        //        shoppingCart.MeasureUnit = product.MeasureUnit;
        //        shoppingCart.AmoCrmId = product.AmoCrmId;

        //        _context.ShoppingCart.Add(shoppingCart);
        //    }

        //    await _context.SaveChangesAsync();

        //    // Возвращаем актуальную корзину пользователя за сегодня
        //    var resultCart = await _context.ShoppingCart
        //        .Where(x => x.UserId == _User.Id && x.OrderDate.Date == today && x.Count > 0)
        //        .ToListAsync();

        //    return resultCart;
        //}

        // POST: api/ShoppingCarts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //[Authorize(Roles = "client")]
        //public async Task<ActionResult<List<ShoppingCart>>> PostShoppingCart(ShoppingCart shoppingCart)
        //{
        //    // Получаем текущую номенклатуру
        //    var nom = await _context.Products.FirstOrDefaultAsync(x => x.Id == shoppingCart.ProductId);
        //    await GetUserInfo();
        //    if (nom != null)
        //    {
        //        shoppingCart.UserId = _User.Id;
        //        shoppingCart.OrderDate = DateTime.Now;

        //        // Дозополняем недостающие данные по номенклатуре
        //        shoppingCart.Price = nom.Price;
        //        shoppingCart.ProductName = nom.Name;
        //        shoppingCart.MeasureUnit = nom.MeasureUnit;
        //        shoppingCart.AmoCrmId = nom.AmoCrmId;

        //        //var shoppingCartJson = JsonSerializer.Serialize(shoppingCart);
        //        //await _cache.SetStringAsync($"cart:{_User.Id}", shoppingCartJson);

        //        _context.ShoppingCart.Add(shoppingCart);
        //        await _context.SaveChangesAsync();
        //        //// Проверим, что суммарно позиции больше нуля
        //        var temp = await _context.ShoppingCart.Where(x => x.ProductId == shoppingCart.ProductId).ToListAsync();
        //        if (temp.Sum(x => x.Count) < 0)
        //        {
        //            _context.ShoppingCart.RemoveRange(temp);
        //            await _context.SaveChangesAsync();
        //        }
        //    }
        //    try
        //    {
        //        var t = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1)).ToListAsync();

        //        // Объединяем общие позиции
        //        var companies = t.GroupBy(p => p.ProductId);
        //        var newShoppingCart = new List<ShoppingCart>();
        //        foreach (var item in companies)
        //        {
        //            ShoppingCart temp = new();
        //            temp = item.FirstOrDefault();
        //            temp.Count = item.Sum(x => x.Count);
        //            newShoppingCart.Add(temp);
        //        }


        //        return newShoppingCart.Where(x => x.Count > 0).ToList();            //return CreatedAtAction("GetShoppingCart", new { id = shoppingCart.Id }, shoppingCart);
        //    }
        //    catch (Exception ex)
        //    {
        //        return NoContent();
        //    }
        //}

        // DELETE: api/ShoppingCarts/5
        //[HttpDelete("{productId}")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> DeleteShoppingCart(string productId)
        //{
        //    await GetUserInfo();
        //    var shoppingCart = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1) & x.ProductId == productId).ToListAsync();
        //    if (shoppingCart == null || shoppingCart.Count == 0)
        //    {
        //        return NotFound();
        //    }

        //    _context.ShoppingCart.RemoveRange(shoppingCart);
        //    await _context.SaveChangesAsync();

        //    return new OkResult();
        //}

        [HttpDelete("{productId}")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> DeleteShoppingCart(string productId)
        {
            await GetUserInfo();
            var userId = _User.Id;
            var today = DateTime.UtcNow.Date;
            var cacheKey = $"cart:{userId}:{today:yyyy-MM-dd}";

            var cachedJson = await _cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(cachedJson))
                return NotFound("Корзина пуста");

            var cart = JsonSerializer.Deserialize<List<ShoppingCart>>(cachedJson);

            var removed = cart.RemoveAll(x => x.ProductId == productId);
            if (removed == 0)
                return NotFound("Товар не найден в корзине");

            // Обновляем Redis
            if (cart.Count > 0)
            {
                var updatedJson = JsonSerializer.Serialize(cart);
                await _cache.SetStringAsync(cacheKey, updatedJson);
            }
            else
            {
                await _cache.RemoveAsync(cacheKey);
            }

            return Ok("Товар удалён из корзины");
        }

        private bool ShoppingCartExists(int id)
        {
            return _context.ShoppingCart.Any(e => e.Id == id);
        }

        async Task GetUserInfo()
        {
            try
            {
                var UserClime = User.FindFirst(ClaimTypes.NameIdentifier);

                // Resolve the user via their email
                if (UserClime != null)
                {
                    _User = await _userManager.FindByIdAsync(UserClime.Value);

                }
                // Get the roles for the user
                if (_User != null)
                {
                    _Roles = await _userManager.GetRolesAsync(_User);

                }

            }
            catch (Exception ex)
            {
                await DBConnector.DBLogs.Error($"Не удалось получить информацию по пользователю", ClaimTypes.NameIdentifier, $"{ex.Message}\n{ex.InnerException}");

            }

        }

    }
}