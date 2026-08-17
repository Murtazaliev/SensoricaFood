using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;

namespace AVBDelivery.Controllers
{
    public class IikoServersController : Controller
    {
        private readonly ApplicationContext _context;

        public IikoServersController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        //[Authorize]
        public async Task<ActionResult<string>> PostTest([FromBody] IikoServer iikoServer)
        //public async Task<IActionResult> PostTest()
        {
            try
            {
                string server = $"{iikoServer.Server}:{iikoServer.Port}";
                if (iikoServer.UseHttps)
                {
                    server = "https://" + server;
                }
                else
                {
                    server = "http://" + server;
                }

                string body = "{\"reportType\":\"SALES\",\"filters\":{\"OpenDate.Typed\":{\"filterType\":\"DateRange\",\"periodType\":\"CUSTOM\",\"from\":\"2023-01-01\",\"to\":\"2023-01-02\"}}}";
                // Заменяем метки времени
                //ConfigServerAPI configServerAPI = new ConfigServerAPI(server, iikoServer.iikoLogin, iikoServer.iikoPassword);
                //var report = await iikoOLAP.GetOLAPByBody(configServerAPI, body);

                //if (report != string.Empty)
                //{
                //    return "Ok";
                //}
                //else
                //{
                //    return null;
                //}
                return null;
            }
            catch (Exception ex)
            {
                //await DBConnector.DBLogs.Error($"Не проверить подключение к iiko", _User.Email, $"{ex.Message}\n{ex.InnerException}");
                return null;
            }

        }


        // GET: IikoServers
        public async Task<IActionResult> Index()
        {
              return View(await _context.IikoServer.ToListAsync());
        }

        // GET: IikoServers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.IikoServer == null)
            {
                return NotFound();
            }

            var iikoServer = await _context.IikoServer
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iikoServer == null)
            {
                return NotFound();
            }

            return View(iikoServer);
        }

        // GET: IikoServers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: IikoServers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Server,UseHttps,Port,Path,iikoLogin,iikoPassword")] IikoServer iikoServer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(iikoServer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iikoServer);
        }

        // GET: IikoServers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.IikoServer == null)
            {
                return NotFound();
            }

            var iikoServer = await _context.IikoServer.FindAsync(id);
            if (iikoServer == null)
            {
                return NotFound();
            }
            return View(iikoServer);
        }

        // POST: IikoServers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Server,UseHttps,Port,Path,iikoLogin,iikoPassword")] IikoServer iikoServer)
        {
            if (id != iikoServer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iikoServer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IikoServerExists(iikoServer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(iikoServer);
        }

        // GET: IikoServers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.IikoServer == null)
            {
                return NotFound();
            }

            var iikoServer = await _context.IikoServer
                .FirstOrDefaultAsync(m => m.Id == id);
            if (iikoServer == null)
            {
                return NotFound();
            }

            return View(iikoServer);
        }

        // POST: IikoServers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.IikoServer == null)
            {
                return Problem("Entity set 'ApplicationContext.IikoServer'  is null.");
            }
            var iikoServer = await _context.IikoServer.FindAsync(id);
            if (iikoServer != null)
            {
                _context.IikoServer.Remove(iikoServer);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IikoServerExists(int id)
        {
          return _context.IikoServer.Any(e => e.Id == id);
        }
    }
}
