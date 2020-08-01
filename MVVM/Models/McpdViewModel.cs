using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mcpdandpcpa.Models
{
    public class McpdViewModel
    {
        public List<string> Ipas { get; set; } = GlobalViewModel.TradingPartners;
        public List<SelectListItem> TradingPartners { get; set; } = GlobalViewModel.TradingPartners.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> JsonFileMode { get; set; } = GlobalViewModel.JsonFileMode.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedJsonFileMode { get; set; }
        public McpdHeader mcpdHeader { get; set; }
        public string JsonExportPath { get; set; }
        public string mcpdJsonMessage { get; set; }
    }
}

