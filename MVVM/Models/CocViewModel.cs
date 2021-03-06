﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mcpdandpcpa.Models
{
    public class CocViewModel
    {
        public List<SelectListItem> DropdownYear = GlobalViewModel.DropdownYear.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> DropdownMonth = GlobalViewModel.DropdownMonth.Select(x=>new SelectListItem { Text=x,Value=x}).ToList();
        public List<SelectListItem> TradingPartners = GlobalViewModel.TradingPartners.Select(x=>new SelectListItem {Text=x,Value=x }).ToList();
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
        public List<McpdContinuityOfCare> CocCurrent { get; set; }
        public List<McpdContinuityOfCare> CocHistory { get; set; }
        public List<McpdContinuityOfCare> CocError { get; set; }
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
        public string CocTypeCurrent { get; set; }
        public string CocTypeHistory { get; set; }
        public string CocTypeError { get; set; }
        public string BenefitTypeCurrent { get; set; }
        public string BenefitTypeHistory { get; set; }
        public string BenefitTypeError { get; set; }
        public string ReceiveDateCurrent { get; set; }
        public string ReceiveDateHistory { get; set; }
        public string ReceiveDateError { get; set; }
        public string CocIdCurrent { get; set; }
        public string CocIdHistory { get; set; }
        public string CocIdError { get; set; }
        public string ParentCocIdCurrent { get; set; }
        public string ParentCocIdHistory { get; set; }
        public string ParentCocIdError { get; set; }
        public string CocDispositionIndCurrent { get; set; }
        public string CocDispositionIndHistory { get; set; }
        public string CocDispositionIndError { get; set; }
        public string ExpirationDateCurrent { get; set; }
        public string ExpirationDateHistory { get; set; }
        public string ExpirationDateError { get; set; }
        public string DenialIndCurrent { get; set; }
        public string DenialIndHistory { get; set; }
        public string DenialIndError { get; set; }
        public string SubmitterNpiCurrent { get; set; }
        public string SubmitterNpiHistory { get; set; }
        public string SubmitterNpiError { get; set; }
        public string ProviderNpiCurrent { get; set; }
        public string ProviderNpiHistory { get; set; }
        public string ProviderNpiError { get; set; }
        public string ProviderTaxonomyCurrent { get; set; }
        public string ProviderTaxonomyHistory { get; set; }
        public string ProviderTaxonomyError { get; set; }
        public string MerExemptionIdCurrent { get; set; }
        public string MerExemptionIdHistory { get; set; }
        public string MerExemptionIdError { get; set; }
        public string ExemptionCodeCurrent { get; set; }
        public string ExemptionCodeHistory { get; set; }
        public string ExemptionCodeError { get; set; }
        public string ExemptionDateCurrent { get; set; }
        public string ExemptionDateHistory { get; set; }
        public string ExemptionDateError { get; set; }
        public string MerDispositionIndCurrent { get; set; }
        public string MerDispositionIndHistory { get; set; }
        public string MerDispositionIndError { get; set; }
        public string MerDispositionDateCurrent { get; set; }
        public string MerDispositionDateHistory { get; set; }
        public string MerDispositionDateError { get; set; }
        public string MerNotMetIndCurrent { get; set; }
        public string MerNotMetIndHistory { get; set; }
        public string MerNotMetIndError { get; set; }
        public List<SelectListItem> CocCascadeDropdown = GlobalViewModel.CocCascadeDropdown.Select(x=>new SelectListItem {Text=x,Value=x }).ToList();
        public string sm11 { get; set; }
        public string sm12 { get; set; }
        public string sm13 { get; set; }
        public string sm14 { get; set; }
        public string sm15 { get; set; }
        public string sm16 { get; set; }
        public string sm17 { get; set; }
        public string sm18 { get; set; }
        public string sm19 { get; set; }
        public string sm20 { get; set; }
        public string sm21 { get; set; }
        public string sm22 { get; set; }
        public string sm23 { get; set; }
        public string sm24 { get; set; }
        public string sm25 { get; set; }
        public string sm26 { get; set; }
        public string sm27 { get; set; }
        public string sm28 { get; set; }
        public string sm29 { get; set; }
        public string sm30 { get; set; }

        public string st11 { get; set; }
        public string st12 { get; set; }
        public string st13 { get; set; }
        public string st14 { get; set; }
        public string st15 { get; set; }
        public string st16 { get; set; }
        public string st17 { get; set; }
        public string st18 { get; set; }
        public string st19 { get; set; }
        public string st20 { get; set; }
        public string st21 { get; set; }
        public string st22 { get; set; }
        public string st23 { get; set; }
        public string st24 { get; set; }
        public string st25 { get; set; }
        public string st26 { get; set; }
        public string st27 { get; set; }
        public string st28 { get; set; }
        public string st29 { get; set; }
        public string st30 { get; set; }

        public string hm11 { get; set; }
        public string hm12 { get; set; }
        public string hm13 { get; set; }
        public string hm14 { get; set; }
        public string hm15 { get; set; }
        public string hm16 { get; set; }
        public string hm17 { get; set; }
        public string hm18 { get; set; }
        public string hm19 { get; set; }
        public string hm20 { get; set; }
        public string hm21 { get; set; }
        public string hm22 { get; set; }
        public string hm23 { get; set; }
        public string hm24 { get; set; }
        public string hm25 { get; set; }
        public string hm26 { get; set; }
        public string hm27 { get; set; }
        public string hm28 { get; set; }
        public string hm29 { get; set; }
        public string hm30 { get; set; }

        public string ht11 { get; set; }
        public string ht12 { get; set; }
        public string ht13 { get; set; }
        public string ht14 { get; set; }
        public string ht15 { get; set; }
        public string ht16 { get; set; }
        public string ht17 { get; set; }
        public string ht18 { get; set; }
        public string ht19 { get; set; }
        public string ht20 { get; set; }
        public string ht21 { get; set; }
        public string ht22 { get; set; }
        public string ht23 { get; set; }
        public string ht24 { get; set; }
        public string ht25 { get; set; }
        public string ht26 { get; set; }
        public string ht27 { get; set; }
        public string ht28 { get; set; }
        public string ht29 { get; set; }
        public string ht30 { get; set; }

        public string em11 { get; set; }
        public string em12 { get; set; }
        public string em13 { get; set; }
        public string em14 { get; set; }
        public string em15 { get; set; }
        public string em16 { get; set; }
        public string em17 { get; set; }
        public string em18 { get; set; }
        public string em19 { get; set; }
        public string em20 { get; set; }
        public string em21 { get; set; }
        public string em22 { get; set; }
        public string em23 { get; set; }
        public string em24 { get; set; }
        public string em25 { get; set; }
        public string em26 { get; set; }
        public string em27 { get; set; }
        public string em28 { get; set; }
        public string em29 { get; set; }
        public string em30 { get; set; }

        public string et11 { get; set; }
        public string et12 { get; set; }
        public string et13 { get; set; }
        public string et14 { get; set; }
        public string et15 { get; set; }
        public string et16 { get; set; }
        public string et17 { get; set; }
        public string et18 { get; set; }
        public string et19 { get; set; }
        public string et20 { get; set; }
        public string et21 { get; set; }
        public string et22 { get; set; }
        public string et23 { get; set; }
        public string et24 { get; set; }
        public string et25 { get; set; }
        public string et26 { get; set; }
        public string et27 { get; set; }
        public string et28 { get; set; }
        public string et29 { get; set; }
        public string et30 { get; set; }

        public List<SelectListItem> PageSizeDropdown = GlobalViewModel.PageSizeDropdown.Select(x=>new SelectListItem {Text=x,Value=x }).ToList();
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
        public string sm20Style { get; set; } = "display:none;";
        public string sm21Style { get; set; } = "display:none;";
        public string sm22Style { get; set; } = "display:none;";
        public string sm23Style { get; set; } = "display:none;";
        public string sm24Style { get; set; } = "display:none;";
        public string sm25Style { get; set; } = "display:none;";
        public string sm26Style { get; set; } = "display:none;";
        public string sm27Style { get; set; } = "display:none;";
        public string sm28Style { get; set; } = "display:none;";
        public string sm29Style { get; set; } = "display:none;";
        public string sm30Style { get; set; } = "display:none;";

        public string hm11Style { get; set; } = "display:none;";
        public string hm12Style { get; set; } = "display:none;";
        public string hm13Style { get; set; } = "display:none;";
        public string hm14Style { get; set; } = "display:none;";
        public string hm15Style { get; set; } = "display:none;";
        public string hm16Style { get; set; } = "display:none;";
        public string hm17Style { get; set; } = "display:none;";
        public string hm18Style { get; set; } = "display:none;";
        public string hm19Style { get; set; } = "display:none;";
        public string hm20Style { get; set; } = "display:none;";
        public string hm21Style { get; set; } = "display:none;";
        public string hm22Style { get; set; } = "display:none;";
        public string hm23Style { get; set; } = "display:none;";
        public string hm24Style { get; set; } = "display:none;";
        public string hm25Style { get; set; } = "display:none;";
        public string hm26Style { get; set; } = "display:none;";
        public string hm27Style { get; set; } = "display:none;";
        public string hm28Style { get; set; } = "display:none;";
        public string hm29Style { get; set; } = "display:none;";
        public string hm30Style { get; set; } = "display:none;";

        public string em11Style { get; set; } = "display:none;";
        public string em12Style { get; set; } = "display:none;";
        public string em13Style { get; set; } = "display:none;";
        public string em14Style { get; set; } = "display:none;";
        public string em15Style { get; set; } = "display:none;";
        public string em16Style { get; set; } = "display:none;";
        public string em17Style { get; set; } = "display:none;";
        public string em18Style { get; set; } = "display:none;";
        public string em19Style { get; set; } = "display:none;";
        public string em20Style { get; set; } = "display:none;";
        public string em21Style { get; set; } = "display:none;";
        public string em22Style { get; set; } = "display:none;";
        public string em23Style { get; set; } = "display:none;";
        public string em24Style { get; set; } = "display:none;";
        public string em25Style { get; set; } = "display:none;";
        public string em26Style { get; set; } = "display:none;";
        public string em27Style { get; set; } = "display:none;";
        public string em28Style { get; set; } = "display:none;";
        public string em29Style { get; set; } = "display:none;";
        public string em30Style { get; set; } = "display:none;";
    }
}
