using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mcpdandpcpa.Models
{
    public class PcpaViewModel
    {
        public List<string> Ipas { get; set; } = GlobalViewModel.TradingPartners;
        public List<SelectListItem> DropdownYear = GlobalViewModel.DropdownYear.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> DropdownMonth = GlobalViewModel.DropdownMonth.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> TradingPartners { get; set; } = GlobalViewModel.TradingPartners.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> DropdownExport = GlobalViewModel.DropdownExport.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string selectedYearHistory { get; set; }
        public string selectedYearError { get; set; }
        public string selectedMonthHistory { get; set; }
        public string selectedMonthError { get; set; }
        public string selectedTradingPartner { get; set; }
        public string selectedTradingPartnerHistory { get; set; }
        public string selectedTradingPartnerError { get; set; }
        public string selectedExport { get; set; }
        public string selectedExportHistory { get; set; }
        public string selectedExportError { get; set; }
        public List<PcpAssignment> PcpaCurrent { get; set; }
        public List<PcpAssignment> PcpaHistory { get; set; }
        public List<PcpAssignment> PcpaError { get; set; }
        public string PageCurrent { get; set; }
        public string PageHistory { get; set; }
        public string PageError { get; set; }
        public string tbPageCurrent { get; set; }
        public string tbPageHistory { get; set; }
        public string tbPageError { get; set; }
        public string PageCurrentTotal { get; set; }
        public string PageHistoryTotal { get; set; }
        public string PageErrorTotal { get; set; }
        public bool currentFirstDisabled { get; set; }
        public bool currentPreviousDisabled { get; set; }
        public bool currentNextDisabled { get; set; }
        public bool currentLastDisabled { get; set; }
        public bool historyFirstDisabled { get; set; }
        public bool historyPreviousDisabled { get; set; }
        public bool historyNextDisabled { get; set; }
        public bool historyLastDisabled { get; set; }
        public bool errorFirstDisabled { get; set; }
        public bool errorPreviousDisabled { get; set; }
        public bool errorNextDisabled { get; set; }
        public bool errorLastDisabled { get; set; }
        public string PlanCodeCurrent { get; set; }
        public string PlanCodeHistory { get; set; }
        public string PlanCodeError { get; set; }
        public string CinCurrent { get; set; }
        public string CinHistory { get; set; }
        public string CinError { get; set; }
        public string NpiCurrent { get; set; }
        public string NpiHistory { get; set; }
        public string NpiError { get; set; }
        public List<SelectListItem> DropdownPlanCodes { get; set; } = GlobalViewModel.DropdownPlanCodes.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public PcpHeader PcpaHeader { get; set; }
        public string JsonExportPath { get; set; }
        public string PcpaMessage { get; set; }
        public string TabActiveCurrent { get; set; }
        public string TabActiveHistory { get; set; }
        public string TabActiveError { get; set; }
        public string TabStyleColorCurrent { get; set; }
        public string TabStyleColorHistory { get; set; }
        public string TabStyleColorError { get; set; }
        public List<SelectListItem> PageSizeDropdown = GlobalViewModel.PageSizeDropdown.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string PageSizeCurrent { get; set; } = GlobalViewModel.PageSizeCurrent;
        public string PageSizeHistory { get; set; } = GlobalViewModel.PageSizeHistory;
        public string PageSizeError { get; set; } = GlobalViewModel.PageSizeError;
        public string DataSourceCurrent { get; set; }
        public string DataSourceHistory { get; set; }
        public string DataSourceError { get; set; }
        public List<SelectListItem> JsonFileMode { get; set; } = GlobalViewModel.JsonFileMode.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string SelectedJsonFileMode { get; set; }
    }
}

