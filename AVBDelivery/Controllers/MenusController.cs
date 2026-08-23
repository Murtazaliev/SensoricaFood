using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Controllers
{
    [Authorize(Roles = "admin")]
    public class MenusController : Controller
    {
        private readonly ApplicationContext _context;

        public MenusController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Menus
        public async Task<IActionResult> Index()
        {
            var menus = await _context.Menus
                .Include(m => m.MenuProducts)
                .OrderByDescending(m => m.Id)
                .ToListAsync();
            return View(menus);
        }

        // GET: Menus/Create
        public IActionResult Create()
        {
            return View(BuildViewModel(null));
        }

        // POST: Menus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuViewModel model, List<string>? ProductIds = null)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildViewModel(model));
            }

            var menu = new Menu
            {
                Name = model.Name,
                IsActive = model.IsActive
            };
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            await SaveProductsAsync(menu.Id, ProductIds);

            var dblog = new DBLog
            {
                Level = "INFO",
                Message = $"Меню \"{menu.Name}\" создано",
                User = User.Identity.Name
            };
            _context.DBLog.Add(dblog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Menus/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menu = await _context.Menus
                .Include(m => m.MenuProducts)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null)
            {
                return NotFound();
            }

            var model = new MenuViewModel
            {
                Id = menu.Id,
                Name = menu.Name,
                IsActive = menu.IsActive
            };

            return View(BuildViewModel(model));
        }

        // POST: Menus/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MenuViewModel model, List<string>? ProductIds = null)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildViewModel(model));
            }

            var menu = await _context.Menus
                .Include(m => m.MenuProducts)
                .FirstOrDefaultAsync(m => m.Id == model.Id);
            if (menu == null)
            {
                return NotFound();
            }

            menu.Name = model.Name;
            menu.IsActive = model.IsActive;

            _context.MenuProducts.RemoveRange(menu.MenuProducts);
            await _context.SaveChangesAsync();

            await SaveProductsAsync(menu.Id, ProductIds);

            var dblog = new DBLog
            {
                Level = "INFO",
                Message = $"Меню \"{menu.Name}\" изменено",
                User = User.Identity.Name
            };
            _context.DBLog.Add(dblog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Menus/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null)
            {
                return NotFound();
            }

            var dblog = new DBLog
            {
                Level = "INFO",
                Message = $"Меню \"{menu.Name}\" удалено",
                User = User.Identity.Name
            };

            _context.Menus.Remove(menu);
            _context.DBLog.Add(dblog);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task SaveProductsAsync(int menuId, List<string>? productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return;
            }

            var uniqueIds = productIds.Distinct().Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            foreach (var productId in uniqueIds)
            {
                _context.MenuProducts.Add(new MenuProduct
                {
                    MenuId = menuId,
                    ProductId = productId
                });
            }
            await _context.SaveChangesAsync();
        }

        private MenuViewModel BuildViewModel(MenuViewModel? model)
        {
            var productGroups = _context.ProductGroups
                .Include(g => g.Products)
                .OrderBy(g => g.GroupName)
                .ToList();

            var selectedIds = new HashSet<string>();
            if (model?.Id != null)
            {
                selectedIds = new HashSet<string>(_context.MenuProducts
                    .Where(mp => mp.MenuId == model.Id)
                    .Select(mp => mp.ProductId));
            }

            var viewModel = new MenuViewModel
            {
                Id = model?.Id,
                Name = model?.Name ?? "",
                IsActive = model?.IsActive ?? true,
                Groups = productGroups.Select(g => new MenuProductGroupViewModel
                {
                    GroupId = g.Id,
                    GroupName = g.GroupName,
                    Products = g.Products.OrderBy(p => p.Name).Select(p => new MenuProductItemViewModel
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        Price = p.Price,
                        IsSelected = selectedIds.Contains(p.Id)
                    }).ToList()
                }).ToList()
            };

            return viewModel;
        }
    }
}