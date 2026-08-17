using System;
using System.Threading.Tasks;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AVBDelivery.Controllers
{
    public class SiteAnnouncementController : Controller
    {
        private ApplicationContext _context;
        private UserManager<User> _userManager;
        public SiteAnnouncementController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var announcement = await _context.SiteAnnouncements.FirstOrDefaultAsync();
                
            if (announcement == null)
            {
                announcement = new SiteAnnouncement()
                {
                    IsEnabled = false,
                    Text = string.Empty
                };
            }
            
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SiteAnnouncement siteAnnouncement)
        {
            if (siteAnnouncement == null)
            {
                return NotFound();
            }
            var currentSiteAnnouncement = await _context.SiteAnnouncements.FirstOrDefaultAsync();
            if (currentSiteAnnouncement != null)
            {
                currentSiteAnnouncement.Text = siteAnnouncement.Text;
                currentSiteAnnouncement.IsEnabled = siteAnnouncement.IsEnabled;
            }
            else
            {
                _context.SiteAnnouncements.Add(siteAnnouncement);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Новость успешно сохранена";
            return RedirectToAction("Index");
        }
    }
}
