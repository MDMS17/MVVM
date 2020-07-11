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
    public class COCController : ControllerBase
    {
        private readonly HistoryContext _context;

        public COCController(HistoryContext context)
        {
            _context = context;
        }

        // GET: api/COC/2020/02
        [HttpGet("{id1}/{id2}")]
        public async Task<ActionResult<IEnumerable<McpdContinuityOfCare>>> GetMcpdContinuityOfCare(long id1, long id2)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.McpdContinuityOfCare.Where(x => x.McpdHeaderId == headerId).ToListAsync();
            }
            return NotFound();
        }
        //Get: api/COC/2020/02/IEHP
        [HttpGet("{id1}/{id2}/{id3:alpha}")]
        public async Task<ActionResult<IEnumerable<McpdContinuityOfCare>>> GetCOCByIpa(long id1, long id2, string id3)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.McpdContinuityOfCare.Where(x => x.McpdHeaderId == headerId && x.TradingPartnerCode == id3).ToListAsync();
            }
            return NotFound();
        }
        // GET: api/COC/5
        [HttpGet("{id}")]
        public async Task<ActionResult<McpdContinuityOfCare>> GetMcpdContinuityOfCare(long id)
        {
            long id2 = _context.McpdContinuityOfCare.Min(x => x.McpdContinuityOfCareId);
            id = id < id2 ? id2 : id;
            var mcpdContinuityOfCare = await _context.McpdContinuityOfCare.FindAsync(id);

            if (mcpdContinuityOfCare == null)
            {
                return NotFound();
            }

            return mcpdContinuityOfCare;
        }

        private bool McpdContinuityOfCareExists(long id)
        {
            return _context.McpdContinuityOfCare.Any(e => e.McpdContinuityOfCareId == id);
        }
    }
}


