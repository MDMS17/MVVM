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
    public class OONController : ControllerBase
    {
        private readonly HistoryContext _context;

        public OONController(HistoryContext context)
        {
            _context = context;
        }

        // GET: api/OON/2020/02
        [HttpGet("{id1}/{id2}")]
        public async Task<ActionResult<IEnumerable<McpdOutOfNetwork>>> GetMcpdOutOfNetwork(long id1, long id2)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.McpdOutOfNetwork.Where(x => x.McpdHeaderId == headerId).ToListAsync();
            }
            return NotFound();
        }
        //Get: api/OON/2020/02/IEHP
        [HttpGet("{id1}/{id2}/{id3:alpha}")]
        public async Task<ActionResult<IEnumerable<McpdOutOfNetwork>>> GetOONByIpa(long id1, long id2, string id3)
        {
            long? headerId = _context.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == id1.ToString() + id2.ToString().PadLeft(2, '0'))?.McpdHeaderId;
            if (headerId.HasValue)
            {
                return await _context.McpdOutOfNetwork.Where(x => x.McpdHeaderId == headerId && x.TradingPartnerCode == id3).ToListAsync();
            }
            return NotFound();
        }
        // GET: api/OON/5
        [HttpGet("{id}")]
        public async Task<ActionResult<McpdOutOfNetwork>> GetMcpdOutOfNetwork(long id)
        {
            long id2 = _context.McpdOutOfNetwork.Min(x => x.McpdOutOfNetworkId);
            id = id < id2 ? id2 : id;
            var mcpdOutOfNetwork = await _context.McpdOutOfNetwork.FindAsync(id);

            if (mcpdOutOfNetwork == null)
            {
                return NotFound();
            }

            return mcpdOutOfNetwork;
        }

        private bool McpdOutOfNetworkExists(long id)
        {
            return _context.McpdOutOfNetwork.Any(e => e.McpdOutOfNetworkId == id);
        }
    }
}

