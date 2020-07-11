using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mcpdandpcpa.Models
{
    public class JsonFileViewModel
    {
        public List<SelectListItem> JsonFileType = GlobalViewModel.JsonFileType.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedFileType { get; set; }
        public string FilePath { get; set; }
        public List<Tuple<bool, string, string, string, string>> SelectedFiles { get; set; }
        public string SelectedItems { get; set; }
    }
}

