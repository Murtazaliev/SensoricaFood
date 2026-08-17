using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace AVBDelivery.Controllers
{
    public class NomenclatureManualController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        User _User;
        IList<string> _Roles;

        public NomenclatureManualController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: NomenclatureManual
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Index()
        {
            var nom = new Nomenclature();
            nom.ProductGroup = await _context.ProductGroups.Include(x => x.Products).ToListAsync();

            //var nom = await _context.Nomenclature.Include(x => x.ProductGroup).ThenInclude(x => x.Products).Include(x => x.Products).ToListAsync();

            await GetUserInfo();
            var order = new Order();
            order.Items = new();
            if (_User != null)
            {


                var shoppingCart = await _context.ShoppingCart.Where(x => x.UserId == _User.Id & x.OrderDate >= DateTime.Today & x.OrderDate < DateTime.Today.AddDays(1)).ToListAsync();

                // Объединяем общие позиции
                foreach (var item in shoppingCart.GroupBy(p => p.ProductId))
                {
                    OrderItem orderItem = new()
                    {
                        Count = item.Sum(x => x.Count),
                        Price = item.FirstOrDefault().Price,
                        ProductName = item.FirstOrDefault().ProductName,
                        ProductId = item.FirstOrDefault().ProductId
                    };
                    if (orderItem.Count > 0)
                    {
                        order.Items.Add(orderItem);
                        order.Sum += orderItem.Price * orderItem.Count;
                    }
                }
            }
            //order.OrderDate = shoppingCart.FirstOrDefault().OrderDate;
            //order.UserId = _User.Id;


            //return View(order);


            ProductListViewModel ret = new() { NomenclatureList = nom, Order = order };
            return View(ret);

            //var nom = new Nomenclature
            //{
            //    ProductGroup = await _context.Productgroup.Where(x => x.groupInManualMode == true).ToListAsync(),
            //    Products = await _context.Products.Where(x => x.productInManualMode == true).ToListAsync()
            //};

            //return View(nom);
        }

        // GET: NomenclatureManual/Details/5
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: NomenclatureManual/Create
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Create()
        {
            var groups = await _context.ProductGroups.OrderBy(x => x.GroupName).ToListAsync();
            Dictionary<string, string> groupNames = new();
            foreach (var item in groups)
            {
                groupNames.Add(item.Id , item.GroupName);
            }
            CreateProductViewModel ret = new()
            {
                Groups = groupNames
            };
            return View(ret);
        }

        // POST: NomenclatureManual/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Create([Bind("Name,ParentGroupName,Price,IsActive")] Product product)
        {
            if (product.Name == null)
            {
                ModelState.AddModelError("Name", "Название номенклатуры не указано.");
            }
            if (ModelState.IsValid)
            {
                product.Id = Guid.NewGuid().ToString();

                var group = await _context.ProductGroups.Include(x => x.Products).Where(x => x.GroupName == product.ParentGroupName).FirstOrDefaultAsync();

                group.Products.Add(product);
                _context.Update(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            CreateProductViewModel ret = new();
            ret.Product = product;
            var groups = await _context.ProductGroups.OrderBy(x => x.GroupName).ToListAsync();
            Dictionary<string, string> groupNames = new();
            foreach (var item in groups)
            {
                groupNames.Add(item.Id, item.GroupName);
            }
            ret.Groups = groupNames;


            return View(ret);
        }

        // GET: NomenclatureManual/Delete/5
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: NomenclatureManual/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'ApplicationContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }









        // GET: NomenclatureManual/Edit/5
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var groups = await _context.ProductGroups.OrderBy(x => x.GroupName).ToListAsync();
            Dictionary<string, string> groupNames = new();
            foreach (var item in groups)
            {
                groupNames.Add(item.Id, item.GroupName);
            }
            CreateProductViewModel ret = new()
            {
                Groups = groupNames,
                Product = product
            };


            return View(ret);
        }

        // POST: NomenclatureManual/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<IActionResult> Edit(Product product)
        {

            var oldProduct = await _context.Products.FindAsync(product.Id);
            if (oldProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    oldProduct.Name = product.Name;
                    oldProduct.Price = product.Price;
                    oldProduct.IsActive = product.IsActive;

                    if (oldProduct.ParentGroupName != product.ParentGroupName)
                    {
                        oldProduct.ParentGroupName = product.ParentGroupName;

                        var oldGroup = await _context.ProductGroups.Include(x => x.Products).Where(x => x.GroupName == oldProduct.ParentGroupName).FirstOrDefaultAsync();
                        oldGroup.Products.Remove(oldProduct);

                        var group = await _context.ProductGroups.Include(x => x.Products).Where(x => x.GroupName == product.ParentGroupName).FirstOrDefaultAsync();
                        group.Products.Add(oldProduct);

                        _context.Update(oldGroup);
                        _context.Update(group);
                    }
                    else
                    {
                        _context.Update(oldProduct);
                    }

                    
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    //if (!EmailTemplateExists(emailTemplate.Id))
                    //{
                    //    return NotFound();
                    //}
                    //else
                    //{
                    //    throw;
                    //}
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
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
