using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using AVBDelivery.ViewModels;

namespace AVBDelivery.Controllers
{
    public class DBLogsController : Controller
    {
        private readonly ApplicationContext _context;

        public DBLogsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: DBLogs
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(string? startDate, string? endDate, int page = 1)
        {
            int pageSize = 20;
            List<DBLog> allLogs;
            if (startDate != null && endDate != null)
            {
                var start = Convert.ToDateTime(startDate);
                var end = Convert.ToDateTime(endDate);
                allLogs = await _context.DBLog.Where(l => l.DateTime >= start && l.DateTime <= end.AddDays(1)).OrderByDescending(l => l.DateTime).ToListAsync();
            }
            else
            {
                allLogs = await _context.DBLog.OrderByDescending(l => l.DateTime).ToListAsync();
            }            
            var logs = allLogs.Skip((page - 1) * pageSize).Take(pageSize);
            var pageInfo = new PageInfo
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = allLogs.Count
            };
            var model = new DBLogViewModel
            {
                DBLogs = logs,
                PageInfo = pageInfo
            };
            return View(model);
        }

        // GET: DBLogs/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DBLog == null)
            {
                return NotFound();
            }

            var dBLog = await _context.DBLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dBLog == null)
            {
                return NotFound();
            }

            return View(dBLog);
        }

        // GET: DBLogs/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: DBLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,DateTime,Level,User,Message,AdditionalInfo")] DBLog dBLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dBLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dBLog);
        }

        // GET: DBLogs/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DBLog == null)
            {
                return NotFound();
            }

            var dBLog = await _context.DBLog.FindAsync(id);
            if (dBLog == null)
            {
                return NotFound();
            }
            return View(dBLog);
        }

        // POST: DBLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateTime,Level,User,Message,AdditionalInfo")] DBLog dBLog)
        {
            if (id != dBLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dBLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DBLogExists(dBLog.Id))
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
            return View(dBLog);
        }

        // GET: DBLogs/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DBLog == null)
            {
                return NotFound();
            }

            var dBLog = await _context.DBLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dBLog == null)
            {
                return NotFound();
            }

            return View(dBLog);
        }

        // POST: DBLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DBLog == null)
            {
                return Problem("Entity set 'ApplicationContext.DBLog'  is null.");
            }
            var dBLog = await _context.DBLog.FindAsync(id);
            if (dBLog != null)
            {
                _context.DBLog.Remove(dBLog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DBLogExists(int id)
        {
          return _context.DBLog.Any(e => e.Id == id);
        }
    }
}
