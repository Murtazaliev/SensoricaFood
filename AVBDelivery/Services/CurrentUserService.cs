using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AVBDelivery.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        private User? _user;
        private IList<string>? _roles;
        private bool _loaded;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public async Task<User?> GetUserAsync()
        {
            if (_loaded) return _user;

            var userId = UserId;
            if (userId != null)
            {
                _user = await _userManager.FindByIdAsync(userId);
                if (_user != null)
                {
                    _roles = await _userManager.GetRolesAsync(_user);
                }
            }
            _loaded = true;
            return _user;
        }

        public async Task<IList<string>> GetRolesAsync()
        {
            if (!_loaded) await GetUserAsync();
            return _roles ?? new List<string>();
        }
    }
}
