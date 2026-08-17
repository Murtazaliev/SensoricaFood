using System;
using System.Threading.Tasks;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Controllers
{
    public class SettingsController : Controller
    {
        private ApplicationContext _context;
        private UserManager<User> _userManager;
        public SettingsController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings == null || string.IsNullOrEmpty(settings.ApiKey))
            {
                settings = new Settings()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApiKey = string.Empty
                };
            }
            
            return View(settings);
        }

        public async Task<IActionResult> Save(Settings settings)
        {
            if (settings == null)
            {
                return NotFound();
            }
            var currentSettings = await _context.Settings.FirstOrDefaultAsync();
            if (currentSettings != null)
            {
                currentSettings.ApiKey = settings.ApiKey;
            }
            else
            {
                _context.Settings.Add(settings);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
