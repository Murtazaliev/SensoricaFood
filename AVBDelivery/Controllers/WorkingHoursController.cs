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
    public class WorkingHoursController : Controller
    {
        private readonly ApplicationContext _context;

        public WorkingHoursController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: WorkingHours
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var t = await _context.WorkingHours.ToListAsync();
            var ret = new List<WorkingHoursViewModel>();
            foreach (var item in t)
            {
                ret.Add(new WorkingHoursViewModel()
                {
                    Description = item.Description,
                    Name = item.Name,
                    Id = item.Id,
                    endTime = new TimeOnly(item.endTime.Hour, item.endTime.Minute),
                    startTime = new TimeOnly(item.startTime.Hour, item.startTime.Minute)

                });
            }
              return View(ret);
        }

        // GET: WorkingHours/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WorkingHours == null)
            {
                return NotFound();
            }

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workingHours == null)
            {
                return NotFound();
            }
            var ret = new WorkingHoursViewModel()
            {
                Description = workingHours.Description,
                Name = workingHours.Name,
                Id = workingHours.Id,
                endTime = new TimeOnly(workingHours.endTime.Hour, workingHours.endTime.Minute),
                startTime = new TimeOnly(workingHours.startTime.Hour, workingHours.startTime.Minute)

            };

            return View(ret);
        }

        // GET: WorkingHours/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create(string returnUrl = null)
        {
            return View(new WorkingHoursViewModel { ReturnUrl = returnUrl });

        }

        // POST: WorkingHours/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,startTime,endTime")] WorkingHoursViewModel workingHours)
        {
            if (ModelState.IsValid)
            {
                WorkingHours wh = new WorkingHours()
                {
                    Description = workingHours.Description,
                    endTime = new DateTime(1,1,1, workingHours.endTime.Hour, workingHours.endTime.Minute, workingHours.endTime.Second),
                    startTime = new DateTime(1, 1, 1, workingHours.startTime.Hour, workingHours.startTime.Minute, workingHours.startTime.Second),
                    Name = workingHours.Name,

                };
                _context.Add(wh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workingHours);
        }

        // GET: WorkingHours/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WorkingHours == null)
            {
                return NotFound();
            }

            var workingHours = await _context.WorkingHours.FindAsync(id);
            if (workingHours == null)
            {
                return NotFound();
            }
            var ret = new WorkingHoursViewModel()
            {
                Description = workingHours.Description,
                Name = workingHours.Name,
                Id = workingHours.Id,
                endTime = new TimeOnly(workingHours.endTime.Hour, workingHours.endTime.Minute),
                startTime = new TimeOnly(workingHours.startTime.Hour, workingHours.startTime.Minute)

            };

            return View(ret);
        }

        // POST: WorkingHours/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,startTime,endTime")] WorkingHoursViewModel workingHours)
        {
            if (id != workingHours.Id)
            {
                return NotFound();
            }
            WorkingHours wh = new WorkingHours()
            {
                Description = workingHours.Description,
                endTime = new DateTime(1, 1, 1, workingHours.endTime.Hour, workingHours.endTime.Minute, workingHours.endTime.Second),
                startTime = new DateTime(1, 1, 1, workingHours.startTime.Hour, workingHours.startTime.Minute, workingHours.startTime.Second),
                Name = workingHours.Name,
                Id= workingHours.Id
            };
            if (ModelState.IsValid)
            {
                try
                {

                    _context.Update(wh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkingHoursExists(workingHours.Id))
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
            return View(wh);
        }

        // GET: WorkingHours/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WorkingHours == null)
            {
                return NotFound();
            }

            var workingHours = await _context.WorkingHours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workingHours == null)
            {
                return NotFound();
            }
            WorkingHoursViewModel wh = new WorkingHoursViewModel()
            {
                Description = workingHours.Description,
                endTime = new TimeOnly(workingHours.endTime.Hour, workingHours.endTime.Minute),
                startTime = new TimeOnly(workingHours.startTime.Hour, workingHours.startTime.Minute),
                Name = workingHours.Name,

            };

            return View(wh);
        }

        // POST: WorkingHours/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WorkingHours == null)
            {
                return Problem("Entity set 'ApplicationContext.WorkingHours'  is null.");
            }
            var workingHours = await _context.WorkingHours.FindAsync(id);
            if (workingHours != null)
            {
                _context.WorkingHours.Remove(workingHours);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkingHoursExists(int id)
        {
          return _context.WorkingHours.Any(e => e.Id == id);
        }
    }
}
