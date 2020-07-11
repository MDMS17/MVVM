using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;

namespace mcpdandpcpa.Controllers
{
    public class DashboardController : Controller
    {
        private readonly LogContext _context;

        public DashboardController(LogContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            return View(await _context.ProcessLogs.ToListAsync());
        }

        // GET: Dashboard/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var processLog = await _context.ProcessLogs
                .FirstOrDefaultAsync(m => m.LogId == id);
            if (processLog == null)
            {
                return NotFound();
            }

            return View(processLog);
        }

        // GET: Dashboard/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dashboard/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogId,TradingPartnerCode,RecordYear,RecordMonth,GrievanceTotal,GrievanceSubmits,GrievanceErrors,AppealTotal,AppealSubmits,AppealErrors,COCTotal,COCSubmits,COCErrors,OONTotal,OONSubmits,OONErrors,PCPATotal,PCPASubmits,PCPAErrors,RunStatus,RunTime,RunBy")] ProcessLog processLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(processLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(processLog);
        }

        // GET: Dashboard/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var processLog = await _context.ProcessLogs.FindAsync(id);
            if (processLog == null)
            {
                return NotFound();
            }
            return View(processLog);
        }

        // POST: Dashboard/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LogId,TradingPartnerCode,RecordYear,RecordMonth,GrievanceTotal,GrievanceSubmits,GrievanceErrors,AppealTotal,AppealSubmits,AppealErrors,COCTotal,COCSubmits,COCErrors,OONTotal,OONSubmits,OONErrors,PCPATotal,PCPASubmits,PCPAErrors,RunStatus,RunTime,RunBy")] ProcessLog processLog)
        {
            if (id != processLog.LogId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(processLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProcessLogExists(processLog.LogId))
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
            return View(processLog);
        }

        // GET: Dashboard/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var processLog = await _context.ProcessLogs
                .FirstOrDefaultAsync(m => m.LogId == id);
            if (processLog == null)
            {
                return NotFound();
            }

            return View(processLog);
        }

        // POST: Dashboard/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var processLog = await _context.ProcessLogs.FindAsync(id);
            _context.ProcessLogs.Remove(processLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProcessLogExists(int id)
        {
            return _context.ProcessLogs.Any(e => e.LogId == id);
        }
    }
}
