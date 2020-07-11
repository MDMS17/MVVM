using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mcpdandpcpa.Models
{
    public class GrievanceViewModel
    {
        public List<SelectListItem> DropdownYear = GlobalViewModel.DropdownYear.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> DropdownMonth = GlobalViewModel.DropdownMonth.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> TradingPartners = GlobalViewModel.TradingPartners.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> DropdownExport = GlobalViewModel.DropdownExport.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string selectedYear { get; set; }
        public string selectedMonth { get; set; }
        public string selectedTradingPartner { get; set; }
        public string selectedExport { get; set; }
        public string selectedExportHistory { get; set; }
        public string selectedExportError { get; set; }
        public string PageCurrent { get; set; }
        public string PageHistory { get; set; }
        public string PageError { get; set; }
        public string tbPageCurrent { get; set; }
        public string tbPageHistory { get; set; }
        public string tbPageError { get; set; }
        public string PageCurrentTotal { get; set; }
        public string PageHistoryTotal { get; set; }
        public string PageErrorTotal { get; set; }
        public string selectedYearHistory { get; set; }
        public string selectedYearError { get; set; }
        public string selectedMonthHistory { get; set; }
        public string selectedMonthError { get; set; }
        public string selectedTradingPartnerHistory { get; set; }
        public string selectedTradingPartnerError { get; set; }
        public List<McpdGrievance> GrievanceCurrent { get; set; }
        public List<McpdGrievance> GrievanceHistory { get; set; }
        public List<McpdGrievance> GrievanceError { get; set; }
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
        public List<SelectListItem> DropdownPlanCodes = GlobalViewModel.DropdownPlanCodes.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string TabActiveCurrent { get; set; }
        public string TabActiveHistory { get; set; }
        public string TabActiveError { get; set; }
        public string TabStyleColorCurrent { get; set; }
        public string TabStyleColorHistory { get; set; }
        public string TabStyleColorError { get; set; }
        public string DataSourceCurrent { get; set; }
        public string DataSourceHistory { get; set; }
        public string DataSourceError { get; set; }
        public string RecordTypeCurrent { get; set; }
        public string RecordTypeHistory { get; set; }
        public string RecordTypeError { get; set; }
        public string GrievanceTypeCurrent { get; set; }
        public string GrievanceTypeHistory { get; set; }
        public string GrievanceTypeError { get; set; }
        public string BenefitTypeCurrent { get; set; }
        public string BenefitTypeHistory { get; set; }
        public string BenefitTypeError { get; set; }
        public string ExemptIndicatorCurrent { get; set; }
        public string ExemptIndicatorHistory { get; set; }
        public string ExemptIndicatorError { get; set; }
        public string ReceiveDateCurrent { get; set; }
        public string ReceiveDateHistory { get; set; }
        public string ReceiveDateError { get; set; }
        public string GrievanceIdCurrent { get; set; }
        public string GrievanceIdHistory { get; set; }
        public string GrievanceIdError { get; set; }
        public string ParentIdCurrent { get; set; }
        public string ParentIdHistory { get; set; }
        public string ParentIdError { get; set; }
        public List<SelectListItem> GrievanceCascadeDropdown = GlobalViewModel.GrievanceCascadeDropdown.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string sm11 { get; set; }
        public string sm12 { get; set; }
        public string sm13 { get; set; }
        public string sm14 { get; set; }
        public string sm15 { get; set; }
        public string sm16 { get; set; }
        public string sm17 { get; set; }
        public string sm18 { get; set; }
        public string sm19 { get; set; }
        public string st11 { get; set; }
        public string st12 { get; set; }
        public string st13 { get; set; }
        public string st14 { get; set; }
        public string st15 { get; set; }
        public string st16 { get; set; }
        public string st17 { get; set; }
        public string st18 { get; set; }
        public string st19 { get; set; }
        public string hm11 { get; set; }
        public string hm12 { get; set; }
        public string hm13 { get; set; }
        public string hm14 { get; set; }
        public string hm15 { get; set; }
        public string hm16 { get; set; }
        public string hm17 { get; set; }
        public string hm18 { get; set; }
        public string hm19 { get; set; }
        public string ht11 { get; set; }
        public string ht12 { get; set; }
        public string ht13 { get; set; }
        public string ht14 { get; set; }
        public string ht15 { get; set; }
        public string ht16 { get; set; }
        public string ht17 { get; set; }
        public string ht18 { get; set; }
        public string ht19 { get; set; }
        public string em11 { get; set; }
        public string em12 { get; set; }
        public string em13 { get; set; }
        public string em14 { get; set; }
        public string em15 { get; set; }
        public string em16 { get; set; }
        public string em17 { get; set; }
        public string em18 { get; set; }
        public string em19 { get; set; }
        public string et11 { get; set; }
        public string et12 { get; set; }
        public string et13 { get; set; }
        public string et14 { get; set; }
        public string et15 { get; set; }
        public string et16 { get; set; }
        public string et17 { get; set; }
        public string et18 { get; set; }
        public string et19 { get; set; }
        public List<SelectListItem> PageSizeDropdown = GlobalViewModel.PageSizeDropdown.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public string PageSizeCurrent { get; set; } = GlobalViewModel.PageSizeCurrent;
        public string PageSizeHistory { get; set; } = GlobalViewModel.PageSizeHistory;
        public string PageSizeError { get; set; } = GlobalViewModel.PageSizeError;
        public string sm11Style { get; set; } = "display:none;";
        public string sm12Style { get; set; } = "display:none;";
        public string sm13Style { get; set; } = "display:none;";
        public string sm14Style { get; set; } = "display:none;";
        public string sm15Style { get; set; } = "display:none;";
        public string sm16Style { get; set; } = "display:none;";
        public string sm17Style { get; set; } = "display:none;";
        public string sm18Style { get; set; } = "display:none;";
        public string sm19Style { get; set; } = "display:none;";
        public string hm11Style { get; set; } = "display:none;";
        public string hm12Style { get; set; } = "display:none;";
        public string hm13Style { get; set; } = "display:none;";
        public string hm14Style { get; set; } = "display:none;";
        public string hm15Style { get; set; } = "display:none;";
        public string hm16Style { get; set; } = "display:none;";
        public string hm17Style { get; set; } = "display:none;";
        public string hm18Style { get; set; } = "display:none;";
        public string hm19Style { get; set; } = "display:none;";
        public string em11Style { get; set; } = "display:none;";
        public string em12Style { get; set; } = "display:none;";
        public string em13Style { get; set; } = "display:none;";
        public string em14Style { get; set; } = "display:none;";
        public string em15Style { get; set; } = "display:none;";
        public string em16Style { get; set; } = "display:none;";
        public string em17Style { get; set; } = "display:none;";
        public string em18Style { get; set; } = "display:none;";
        public string em19Style { get; set; } = "display:none;";
    }
}


