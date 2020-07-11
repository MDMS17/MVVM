using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCPDIP.Api.Data;
using mcpdipData;

namespace MCPDIP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppealsController : ControllerBase
    {
        private readonly HistoryContext _context;

        public AppealsController(HistoryContext context)
        {
            _context = context;
        }

        // GET: api/Appeals/2020/02
        [HttpGet("{id1}/{id2}")]
        public async Task<ActionResult<IEnumerable<McpdAppeal>>> GetAppeals(long id1, long id2)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.Appeals.Where(x => x.McpdHeaderId == headerId).ToListAsync();
            }
            return NotFound();
        }
        //Get api/Appeals/2020/02/IEHP
        [HttpGet("{id1}/{id2}/{id3:alpha}")]
        public async Task<ActionResult<IEnumerable<McpdAppeal>>> GetAppealsByIpa(long id1, long id2, string id3)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.Appeals.Where(x => x.McpdHeaderId == headerId && x.TradingPartnerCode == id3).ToListAsync();
            }
            return NotFound();
        }

        // GET: api/Appeals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<McpdAppeal>> GetMcpdAppeal(long id)
        {
            long id2 = _context.Appeals.Min(x => x.McpdAppealId);
            id = id < id2 ? id2 : id;
            var mcpdAppeal = await _context.Appeals.FindAsync(id);

            if (mcpdAppeal == null)
            {
                return NotFound();
            }

            return mcpdAppeal;
        }

        private bool McpdAppealExists(long id)
        {
            return _context.Appeals.Any(e => e.McpdAppealId == id);
        }
    }
}

