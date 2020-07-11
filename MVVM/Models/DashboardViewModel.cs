using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;

namespace mcpdandpcpa.Models
{
    public class DashboardViewModel
    {
        public List<ProcessLogDisplayModel> ProcessLogs { get; set; }
        public List<SubmissionLogDisplayModel> SubmissionLogs { get; set; }
    }
}


