using mcpdipData;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public List<ItemDetail> ResponseDetails { get; set; }
        public List<SelectListItem> DropdownItems { get; set; } = GlobalViewModel.DropdownItems.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedItem { get; set; }
        public List<SelectListItem> DropdownYear = GlobalViewModel.DropdownYear.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedYear { get; set; }
        public List<SelectListItem> DropdownMonth = GlobalViewModel.DropdownMonth.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedMonth { get; set; }
        public string SelectedDataSource { get; set; }
        public List<SelectListItem> DropdownExport { get; set; } = GlobalViewModel.DropdownExport.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedExport { get; set; }
        public List<SelectListItem> DropdownSeverity { get; set; } = GlobalViewModel.DropdownSeverity.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedSeverity { get; set; }
        public bool currentFirstDisabled { get; set; }
        public bool currentPreviousDisabled { get; set; }
        public bool currentNextDisabled { get; set; }
        public bool currentLastDisabled { get; set; }
        public string PageCurrent { get; set; }
        public string tbPageCurrent { get; set; }
        public string PageCurrentTotal { get; set; }
        public string PageSizeCurrent { get; set; } = GlobalViewModel.PageSizeCurrent;
        public List<SelectListItem> PageSizeDropdown { get; set; } = GlobalViewModel.PageSizeDropdown.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
    }
}

