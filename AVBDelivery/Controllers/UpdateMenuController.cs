using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AVBDelivery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateMenuController : ControllerBase
    {

        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        User _User;
        IList<string> _Roles;

        public UpdateMenuController(ApplicationContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET api/<UpdateMenuController>/5
        [HttpGet("{id}")]
        [Authorize]
        [Authorize(Roles = "nomenclatureEditor")]
        public async Task<string> Get(string id)
        {
            var nom = await _context.Products.FirstOrDefaultAsync(x=>x.Id == id);
            if (nom == null)
            {
                return "Номенклатура не найдена.";
            }
            else 
            {
                nom.IsActive = !nom.IsActive;
                _context.Update(nom);
                await _context.SaveChangesAsync();
                return $"Номенклатура \"{nom.Name}\" обновлена.";  
            }
            //return null;
        }




    }
}
