using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AVBDelivery.Models;
using AVBDelivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace AVBDelivery.Controllers
{
    public class DateOfComingController : Controller
    {
        private readonly ApplicationContext _context;

        public DateOfComingController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: WorkingHours
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var ret = await _context.DateOfComing.ToListAsync();
            return View(ret);
        }

        // GET: WorkingHours/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DateOfComing == null)
            {
                return NotFound();
            }

            var dateOfComing = await _context.DateOfComing
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dateOfComing == null)
            {
                return NotFound();
            }

            return View(dateOfComing);
        }

        // GET: WorkingHours/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();

        }

        // POST: WorkingHours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,TimeOnly")] DateOfComing dateOfComing)
        {
            if (ModelState.IsValid)
            {
                DateOfComing wh = new DateOfComing()
                {
                    TimeOnly = dateOfComing.TimeOnly
                };
                _context.Add(wh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dateOfComing);
        }

        // GET: WorkingHours/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DateOfComing == null)
            {
                return NotFound();
            }

            var dateOfComing = await _context.DateOfComing.FindAsync(id);
            if (dateOfComing == null)
            {
                return NotFound();
            }
            return View(dateOfComing);
        }

        // POST: WorkingHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TimeOnly")] DateOfComing dateOfComing)
        {
            if (id != dateOfComing.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dateOfComing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DateOfComingExists(dateOfComing.Id))
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
            return View(dateOfComing);
        }

        // GET: WorkingHours/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DateOfComing == null)
            {
                return NotFound();
            }

            var dateOfComing = await _context.DateOfComing
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dateOfComing == null)
            {
                return NotFound();
            }
            return View(dateOfComing);
        }

        // POST: WorkingHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DateOfComing == null)
            {
                return Problem("Entity set 'ApplicationContext.DateOfComing'  is null.");
            }
            var dateOfComing = await _context.DateOfComing.FindAsync(id);
            if (dateOfComing != null)
            {
                _context.DateOfComing.Remove(dateOfComing);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DateOfComingExists(int id)
        {
          return _context.DateOfComing.Any(e => e.Id == id);
        }
    }
}
