using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AVBDelivery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsApiController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        User _User;
        IList<string> _Roles;

        public OrganizationsApiController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api
        [HttpGet("{id}")]
        [Authorize(Roles = "client")]
        public async Task<ActionResult<Organization>> GetDeliveryMinimalSum(string id)
        {
            var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == id);
            return organization;
            //return (await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationId == id))?.MinimalSum ?? 0;
        }

    }
}