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
    public class PCPAController : ControllerBase
    {
        private readonly HistoryContext _context;

        public PCPAController(HistoryContext context)
        {
            _context = context;
        }

        // GET: api/PCPA/2020/02
        [HttpGet("{id1}/{id2}")]
        public async Task<ActionResult<IEnumerable<PcpAssignment>>> GetPcpAssignments(long id1, long id2)
        {
            long? headerId = _context.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.PcpHeaderId;
            if (headerId.HasValue)
            {
                return await _context.PcpAssignments.Where(x => x.PcpHeaderId == headerId).ToListAsync();
            }
            return NotFound();
        }
        //Get: api/PCPA/2020/02/IEHP
        [HttpGet("{id1}/{id2}/{id3:alpha}")]
        public async Task<ActionResult<IEnumerable<PcpAssignment>>> GetPcpAssignmentByIpa(long id1, long id2, string id3)
        {
            long? headerId = _context.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.PcpHeaderId;
            if (headerId.HasValue)
            {
                return await _context.PcpAssignments.Where(x => x.PcpHeaderId == headerId && x.TradingPartnerCode == id3).ToListAsync();
            }
            return NotFound();
        }
        // GET: api/PCPA/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PcpAssignment>> GetPcpAssignment(long id)
        {
            long id2 = _context.PcpAssignments.Min(x => x.PcpAssignmentId);
            id = id < id2 ? id2 : id;
            var pcpAssignment = await _context.PcpAssignments.FindAsync(id);

            if (pcpAssignment == null)
            {
                return NotFound();
            }

            return pcpAssignment;
        }

        private bool PcpAssignmentExists(long id)
        {
            return _context.PcpAssignments.Any(e => e.PcpAssignmentId == id);
        }
    }
}
