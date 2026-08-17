using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AVBDelivery.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;


        UserManager<User> _userManager;
        User _User;
        IList<string> _Roles;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context, UserManager<User> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        [Authorize]

        public IActionResult Index()
        {
            return RedirectToAction("index", "Orders");

            //return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
