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
    public class GrievancesController : ControllerBase
    {
        private readonly HistoryContext _context;

        public GrievancesController(HistoryContext context)
        {
            _context = context;
        }
        // Get: api/Grievances/2020/02
        [HttpGet("{id1}/{id2}")]
        public async Task<ActionResult<IEnumerable<McpdGrievance>>> GetGrievancesByPeriod(long id1, long id2)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.Grievances.Where(x => x.McpdHeaderId == headerId).ToListAsync();
            }
            return NotFound();
        }
        //Get: api/Grievance/2020/02/IEHP
        [HttpGet("{id1}/{id2}/{id3:alpha}")]
        public async Task<ActionResult<IEnumerable<McpdGrievance>>> GetGrievanceByIpa(long id1, long id2, string id3)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.Grievances.Where(x => x.McpdHeaderId == headerId && x.TradingPartnerCode == id3).ToListAsync();
            }
            return NotFound();
        }

        // GET: api/Grievances/5
        [HttpGet("{id}")]
        public async Task<ActionResult<McpdGrievance>> GetMcpdGrievance(long id)
        {
            long id2 = _context.Grievances.Min(x => x.McpdGrievanceId);
            id = id < id2 ? id2 : id;
            var mcpdGrievance = await _context.Grievances.FindAsync(id);

            if (mcpdGrievance == null)
            {
                return NotFound();
            }

            return mcpdGrievance;
        }

        private bool McpdGrievanceExists(long id)
        {
            return _context.Grievances.Any(e => e.McpdGrievanceId == id);
        }
    }
}
