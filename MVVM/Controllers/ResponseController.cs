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
    public class ResponseController : Controller
    {
        private readonly ResponseContext _context;

        public ResponseController(ResponseContext context)
        {
            _context = context;
        }

        // GET: Response
        public async Task<IActionResult> Index()
        {
            ResponseFile pcpaResponseFile = new ResponseFile();

            pcpaResponseFile.responseHierarchy = new List<ResponseHierarchy>();
            pcpaResponseFile.responseHierarchy.Add(new ResponseHierarchy { levelIdentifier = "File", sectionIdentifier = null, responses = new List<ResponseDetail>(), children = new List<ResponseChildren>() });

            pcpaResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "Header", sectionIdentifier = null, responses = new List<ResponseDetail>() });
            pcpaResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "PCPA", sectionIdentifier = null, responses = new List<ResponseDetail>() });


            string ss2 = System.IO.File.ReadAllText(@"\\iehpds6\itvol\EDI\MichaelYan\JosnResponse\IEHP_PCPA_20200706_00001_RESP_RPT000.json");
            pcpaResponseFile = JsonLib.JsonDeserialize.DeserializeReponseFile(ref ss2);

            return View(await _context.McpdipHeaders.ToListAsync());
        }

        // GET: Response/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders
                .FirstOrDefaultAsync(m => m.HeaderId == id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }

            return View(mcpdipHeader);
        }

        // GET: Response/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Response/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HeaderId,FileName,FileType,SubmitterName,SubmissionDate,ValidationStatus,Levels,ResponseHierarchy,SchemaVersion,RecordYear,RecordMonth")] McpdipHeader mcpdipHeader)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mcpdipHeader);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdipHeader);
        }

        // GET: Response/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders.FindAsync(id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }
            return View(mcpdipHeader);
        }

        // POST: Response/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HeaderId,FileName,FileType,SubmitterName,SubmissionDate,ValidationStatus,Levels,ResponseHierarchy,SchemaVersion,RecordYear,RecordMonth")] McpdipHeader mcpdipHeader)
        {
            if (id != mcpdipHeader.HeaderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mcpdipHeader);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!McpdipHeaderExists(mcpdipHeader.HeaderId))
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
            return View(mcpdipHeader);
        }

        // GET: Response/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders
                .FirstOrDefaultAsync(m => m.HeaderId == id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }

            return View(mcpdipHeader);
        }

        // POST: Response/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mcpdipHeader = await _context.McpdipHeaders.FindAsync(id);
            _context.McpdipHeaders.Remove(mcpdipHeader);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool McpdipHeaderExists(int id)
        {
            return _context.McpdipHeaders.Any(e => e.HeaderId == id);
        }
    }
}

