using mcpdipData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mcpdandpcpa.Models
{
    public class ResponseViewModel
    {
        public string FilePath { get; set; }
        public string ArchivePath { get; set; }
        public List<string> SelectedFiles { get; set; }
        public string ButtonDisabled { get; set; }
        public string Message { get; set; }
        public List<ResponseLogDisplayModel> ResponseHeaders { get; set; }
    }
}

