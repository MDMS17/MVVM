using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;
using mcpdandpcpa.Extensions;
using JsonLib;
using ClosedXML.Excel;
using System.IO;

namespace mcpdandpcpa.Controllers
{
    public class CocController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public CocController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }
        public async Task<IActionResult> Index()
        {
            CocViewModel model = new CocViewModel();
            model.CocCurrent = await _context.McpdContinuityOfCare.Take(int.Parse(model.PageSizeCurrent)).ToListAsync();
            model.PageCurrent = "1";
            model.PageCurrentTotal = Math.Ceiling((decimal)_context.McpdContinuityOfCare.Count() / int.Parse(model.PageSizeCurrent)).ToString();
            model.tbPageCurrent = "1";
            model.currentFirstDisabled = true;
            model.currentPreviousDisabled = true;
            model.currentNextDisabled = false;
            model.currentLastDisabled = false;
            model.TabActiveCurrent = "active";
            model.TabActiveHistory = "";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:purple;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:black;";
            var historyHeader = await _contextHistory.McpdHeaders.OrderByDescending(x => x.McpdHeaderId).FirstOrDefaultAsync();
            if (historyHeader != null)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.selectedYearHistory = historyHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthHistory = historyHeader.ReportingPeriod.Substring(4, 2);
                model.CocHistory = await GetCocRecordsHistory(0, historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetCocHistoryPageTotal(historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.tbPageHistory = "1";
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                model.historyNextDisabled = false;
                model.historyLastDisabled = false;
            }
            else
            {
                model.CocHistory = new List<McpdContinuityOfCare>();
            }
            var errorHeader = await _contextError.McpdHeaders.OrderByDescending(x => x.McpdHeaderId).FirstOrDefaultAsync();
            if (errorHeader != null)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.selectedYearError = errorHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthError = errorHeader.ReportingPeriod.Substring(4, 2);
                model.CocError = await GetCocRecordsError(0, errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetCocErrorPageTotal(errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
                model.tbPageError = "1";
                model.errorFirstDisabled = true;
                model.errorPreviousDisabled = true;
                if (int.Parse(model.PageErrorTotal) > 1)
                {
                    model.errorNextDisabled = false;
                    model.errorLastDisabled = false;
                }
                else
                {
                    model.errorNextDisabled = true;
                    model.errorLastDisabled = true;
                }
            }
            else
            {
                model.CocError = new List<McpdContinuityOfCare>();
            }
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? id, CocViewModel model)
        {
            if (id == 103)
            {
                //current next
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
                else
                {
                    int pageCurrent = int.Parse(model.PageCurrent);
                    int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
                    model.CocCurrent = await GetCocRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
                    ModelState["PageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    ModelState["tbPageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    if (pageCurrent > 0)
                    {
                        model.currentFirstDisabled = false;
                        model.currentPreviousDisabled = false;
                        model.currentNextDisabled = false;
                        model.currentLastDisabled = false;
                    }
                    if (pageCurrent == int.Parse(model.PageCurrentTotal) - 1)
                    {
                        model.currentNextDisabled = true;
                        model.currentLastDisabled = true;
                    }
                    model.TabActiveCurrent = "active";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:purple;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 104)
            {
                //current last
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
                else
                {
                    int pageCurrent = int.Parse(model.PageCurrentTotal) - 1;
                    int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
                    model.CocCurrent = await GetCocRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
                    ModelState["PageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    ModelState["tbPageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    model.currentFirstDisabled = false;
                    model.currentPreviousDisabled = false;
                    model.currentNextDisabled = true;
                    model.currentLastDisabled = true;
                    model.TabActiveCurrent = "active";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:purple;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 105)
            {
                //current goto
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
                else
                {
                    int pageCurrent = int.Parse(model.tbPageCurrent) - 1;
                    int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
                    model.CocCurrent = await GetCocRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
                    ModelState["PageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    ModelState["tbPageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    if (pageCurrent == 0)
                    {
                        model.currentFirstDisabled = true;
                        model.currentPreviousDisabled = true;
                        model.currentNextDisabled = false;
                        model.currentLastDisabled = false;
                    }
                    else if (pageCurrent == int.Parse(model.PageCurrentTotal) - 1)
                    {
                        model.currentFirstDisabled = false;
                        model.currentPreviousDisabled = false;
                        model.currentNextDisabled = true;
                        model.currentLastDisabled = true;
                    }
                    else
                    {
                        model.currentFirstDisabled = false;
                        model.currentPreviousDisabled = false;
                        model.currentNextDisabled = false;
                        model.currentLastDisabled = false;
                    }
                    model.TabActiveCurrent = "active";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:purple;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 102)
            {
                //current previous
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
                else
                {
                    int pageCurrent = int.Parse(model.PageCurrent) - 2;
                    int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
                    model.CocCurrent = await GetCocRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
                    ModelState["PageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    ModelState["tbPageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    if (pageCurrent < int.Parse(model.PageCurrentTotal) - 1)
                    {
                        model.currentFirstDisabled = false;
                        model.currentPreviousDisabled = false;
                        model.currentNextDisabled = false;
                        model.currentLastDisabled = false;
                    }
                    if (pageCurrent == 0)
                    {
                        model.currentFirstDisabled = true;
                        model.currentPreviousDisabled = true;
                    }
                    model.TabActiveCurrent = "active";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:purple;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 101)
            {
                //current first
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
                else
                {
                    int pageCurrent = 0;
                    int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
                    model.CocCurrent = await GetCocRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
                    ModelState["PageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    ModelState["tbPageCurrent"].RawValue = (pageCurrent + 1).ToString();
                    model.currentFirstDisabled = true;
                    model.currentPreviousDisabled = true;
                    model.currentNextDisabled = false;
                    model.currentLastDisabled = false;
                    model.TabActiveCurrent = "active";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:purple;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 1)
            {
                //current refresh
                model = await PageSizeChangeCurrent(model);
            }
            else if (id == 106)
            {
                //current page size change
                if (model.PageSizeCurrent != GlobalViewModel.PageSizeCurrent)
                {
                    model = await PageSizeChangeCurrent(model);
                }
            }
            else if (id == 203)
            {
                //history next
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
                else
                {
                    int pageHistory = int.Parse(model.PageHistory);
                    int pageSizeHistory = int.Parse(model.PageSizeHistory);
                    long historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).McpdHeaderId;
                    model.CocHistory = await GetCocRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                    ModelState["PageHistory"].RawValue = (pageHistory + 1).ToString();
                    ModelState["tbPageHistory"].RawValue = (pageHistory + 1).ToString();
                    if (pageHistory > 0)
                    {
                        model.historyFirstDisabled = false;
                        model.historyPreviousDisabled = false;
                        model.historyNextDisabled = false;
                        model.historyLastDisabled = false;
                    }
                    if (pageHistory == int.Parse(model.PageHistoryTotal) - 1)
                    {
                        model.historyNextDisabled = true;
                        model.historyLastDisabled = true;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "active";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:purple;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 204)
            {
                //history last
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
                else
                {
                    int pageHistory = int.Parse(model.PageHistoryTotal) - 1;
                    int pageSizeHistory = int.Parse(model.PageSizeHistory);
                    long historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).McpdHeaderId;
                    model.CocHistory = await GetCocRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                    ModelState["PageHistory"].RawValue = (pageHistory + 1).ToString();
                    ModelState["tbPageHistory"].RawValue = (pageHistory + 1).ToString();
                    model.historyFirstDisabled = false;
                    model.historyPreviousDisabled = false;
                    model.historyNextDisabled = true;
                    model.historyLastDisabled = true;
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "active";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:purple;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 205)
            {
                //history goto
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
                else
                {
                    int pageHistory = int.Parse(model.tbPageHistory) - 1;
                    int pageSizeHistory = int.Parse(model.PageSizeHistory);
                    long historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).McpdHeaderId;
                    model.CocHistory = await GetCocRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                    ModelState["PageHistory"].RawValue = (pageHistory + 1).ToString();
                    ModelState["tbPageHistory"].RawValue = (pageHistory + 1).ToString();
                    if (pageHistory == 0)
                    {
                        model.historyFirstDisabled = true;
                        model.historyPreviousDisabled = true;
                        model.historyNextDisabled = false;
                        model.historyLastDisabled = false;
                    }
                    else if (pageHistory == int.Parse(model.PageHistoryTotal) - 1)
                    {
                        model.historyFirstDisabled = false;
                        model.historyPreviousDisabled = false;
                        model.historyNextDisabled = true;
                        model.historyLastDisabled = true;
                    }
                    else
                    {
                        model.historyFirstDisabled = false;
                        model.historyPreviousDisabled = false;
                        model.historyNextDisabled = false;
                        model.historyLastDisabled = false;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "active";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:purple;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 202)
            {
                //history previous
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
                else
                {
                    int pageHistory = int.Parse(model.PageHistory) - 2;
                    int pageSizeHistory = int.Parse(model.PageSizeHistory);
                    long historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).McpdHeaderId;
                    model.CocHistory = await GetCocRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                    ModelState["PageHistory"].RawValue = (pageHistory + 1).ToString();
                    ModelState["tbPageHistory"].RawValue = (pageHistory + 1).ToString();
                    if (pageHistory < int.Parse(model.PageHistoryTotal) - 1)
                    {
                        model.historyFirstDisabled = false;
                        model.historyPreviousDisabled = false;
                        model.historyNextDisabled = false;
                        model.historyLastDisabled = false;
                    }
                    if (pageHistory == 0)
                    {
                        model.historyFirstDisabled = true;
                        model.historyPreviousDisabled = true;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "active";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:purple;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 201)
            {
                //history first
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
                else
                {
                    int pageHistory = 0;
                    int pageSizeHistory = int.Parse(model.PageSizeHistory);
                    long historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).McpdHeaderId;
                    model.CocHistory = await GetCocRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                    ModelState["PageHistory"].RawValue = (pageHistory + 1).ToString();
                    ModelState["tbPageHistory"].RawValue = (pageHistory + 1).ToString();
                    model.historyFirstDisabled = true;
                    model.historyPreviousDisabled = true;
                    model.historyNextDisabled = false;
                    model.historyLastDisabled = false;
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "active";
                    model.TabActiveError = "";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:purple;";
                    model.TabStyleColorError = "color:black;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
                    if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 2)
            {
                //history refresh
                model = await PageSizeChangeHistory(model);
            }
            else if (id == 206)
            {
                //history page size change
                if (model.PageSizeHistory != GlobalViewModel.PageSizeHistory)
                {
                    model = await PageSizeChangeHistory(model);
                }
            }
            else if (id == 303)
            {
                //error next
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
                else
                {
                    int pageError = int.Parse(model.PageError);
                    int pageSizeError = int.Parse(model.PageSizeError);
                    long errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).McpdHeaderId;
                    model.CocError = await GetCocRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                    ModelState["PageError"].RawValue = (pageError + 1).ToString();
                    ModelState["tbPageError"].RawValue = (pageError + 1).ToString();
                    if (pageError > 0)
                    {
                        model.errorFirstDisabled = false;
                        model.errorPreviousDisabled = false;
                        model.errorNextDisabled = false;
                        model.errorLastDisabled = false;
                    }
                    if (pageError == int.Parse(model.PageErrorTotal) - 1)
                    {
                        model.errorNextDisabled = true;
                        model.errorLastDisabled = true;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "active";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:purple;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 304)
            {
                //error last
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
                else
                {
                    int pageError = int.Parse(model.PageErrorTotal) - 1;
                    int pageSizeError = int.Parse(model.PageSizeError);
                    long errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).McpdHeaderId;
                    model.CocError = await GetCocRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                    ModelState["PageError"].RawValue = (pageError + 1).ToString();
                    ModelState["tbPageError"].RawValue = (pageError + 1).ToString();
                    model.errorFirstDisabled = false;
                    model.errorPreviousDisabled = false;
                    model.errorNextDisabled = true;
                    model.errorLastDisabled = true;
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "active";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:purple;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 305)
            {
                //error goto
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
                else
                {
                    int pageError = int.Parse(model.tbPageError) - 1;
                    int pageSizeError = int.Parse(model.PageSizeError);
                    long errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).McpdHeaderId;
                    model.CocError = await GetCocRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                    ModelState["PageError"].RawValue = (pageError + 1).ToString();
                    ModelState["tbPageError"].RawValue = (pageError + 1).ToString();
                    if (pageError == 0)
                    {
                        model.errorFirstDisabled = true;
                        model.errorPreviousDisabled = true;
                        model.errorNextDisabled = false;
                        model.errorLastDisabled = false;
                    }
                    else if (pageError == int.Parse(model.PageErrorTotal) - 1)
                    {
                        model.errorFirstDisabled = false;
                        model.errorPreviousDisabled = false;
                        model.errorNextDisabled = true;
                        model.errorLastDisabled = true;
                    }
                    else
                    {
                        model.errorFirstDisabled = false;
                        model.errorPreviousDisabled = false;
                        model.errorNextDisabled = false;
                        model.errorLastDisabled = false;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "active";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:purple;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 302)
            {
                //error previous
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
                else
                {
                    int pageError = int.Parse(model.PageError) - 2;
                    int pageSizeError = int.Parse(model.PageSizeError);
                    long errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).McpdHeaderId;
                    model.CocError = await GetCocRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                    ModelState["PageError"].RawValue = (pageError + 1).ToString();
                    ModelState["tbPageError"].RawValue = (pageError + 1).ToString();
                    if (pageError < int.Parse(model.PageErrorTotal) - 1)
                    {
                        model.errorFirstDisabled = false;
                        model.errorPreviousDisabled = false;
                        model.errorNextDisabled = false;
                        model.errorLastDisabled = false;
                    }
                    if (pageError == 0)
                    {
                        model.errorFirstDisabled = true;
                        model.errorPreviousDisabled = true;
                    }
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "active";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:purple;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 301)
            {
                //error first
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
                else
                {
                    int pageError = 0;
                    int pageSizeError = int.Parse(model.PageSizeError);
                    long errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).McpdHeaderId;
                    model.CocError = await GetCocRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                    ModelState["PageError"].RawValue = (pageError + 1).ToString();
                    ModelState["tbPageError"].RawValue = (pageError + 1).ToString();
                    model.errorFirstDisabled = true;
                    model.errorPreviousDisabled = true;
                    model.errorNextDisabled = false;
                    model.errorLastDisabled = false;
                    model.TabActiveCurrent = "";
                    model.TabActiveHistory = "";
                    model.TabActiveError = "active";
                    model.TabStyleColorCurrent = "color:black;";
                    model.TabStyleColorHistory = "color:black;";
                    model.TabStyleColorError = "color:purple;";
                    HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
                    model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
                    if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
                    model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
                    if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
                }
            }
            else if (id == 3)
            {
                //error refresh
                model = await PageSizeChangeError(model);
            }
            else if (id == 306)
            {
                //error page size change
                if (model.PageSizeError != GlobalViewModel.PageSizeError)
                {
                    model = await PageSizeChangeError(model);
                }
            }
            else if (id == 991)
            {
                //this is for test only
            }
            return View(model);
        }

        private async Task<List<McpdContinuityOfCare>> GetCocRecords(int pageCurrent, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            var result = _context.McpdContinuityOfCare.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Skip(pageCurrent * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetCocPageTotal(string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_context.McpdContinuityOfCare.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdContinuityOfCare>> GetCocRecordsHistory(int pageNumber, long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            var result = _contextHistory.McpdContinuityOfCare.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetCocHistoryPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextHistory.McpdContinuityOfCare.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdContinuityOfCare>> GetCocRecordsError(int pageNumber, long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            var result = _contextError.McpdContinuityOfCare.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetCocErrorPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextError.McpdContinuityOfCare.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdContinuityOfCare>> GetCocForDownload(string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource)
        {
            var result = _context.McpdContinuityOfCare.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdContinuityOfCare>> GetCocHistoryForDownload(long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource)
        {
            var result = _contextHistory.McpdContinuityOfCare.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdContinuityOfCare>> GetCocErrorForDownload(long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string CocId, string RecordType, string ParentCocId, string ReceiveDate, string CocType, string BenefitType, string DispositionInd, string ExpirationDate, string DenialInd, string SubmitterNpi, string ProviderNpi, string ProviderTaxonomy, string ExemptionId, string ExemptionDenialCode, string ExemptionDenialDate, string MerDispositionInd, string MerDispositionDate, string MerCocNotMetInd, string DataSource)
        {
            var result = _contextError.McpdContinuityOfCare.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByCocId(CocId).FilterByRecordType(RecordType).FilterByParentCocId(ParentCocId).FilterByCocReceiveDate(ReceiveDate)
                .FilterByCocType(CocType).FilterByBenefitType(BenefitType).FilterByCocDispositionIndicator(DispositionInd)
                .FilterByCocExpirationDate(ExpirationDate).FilterByCocDenialReasonIndicator(DenialInd).FilterBySubmittingProviderNpi(SubmitterNpi)
                .FilterByCocProviderNpi(ProviderNpi).FilterByProviderTaxonomy(ProviderTaxonomy).FilterByMerExemptionId(ExemptionId)
                .FilterByExemtionCode(ExemptionDenialCode).FilterByExemptionDate(ExemptionDenialDate).FilterByMerDispositionInd(MerDispositionDate)
                .FilterByMerDispositionDate(MerDispositionDate).FilterByMerCocNotMetInd(MerCocNotMetInd).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<CocViewModel> PageSizeChangeCurrent(CocViewModel model)
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();
            items.Add(Tuple.Create(model.sm11, model.st11));
            items.Add(Tuple.Create(model.sm12, model.st12));
            items.Add(Tuple.Create(model.sm13, model.st13));
            items.Add(Tuple.Create(model.sm14, model.st14));
            items.Add(Tuple.Create(model.sm15, model.st15));
            items.Add(Tuple.Create(model.sm16, model.st16));
            items.Add(Tuple.Create(model.sm17, model.st17));
            items.Add(Tuple.Create(model.sm18, model.st18));
            items.Add(Tuple.Create(model.sm19, model.st19));
            items.Add(Tuple.Create(model.sm20, model.st20));
            items.Add(Tuple.Create(model.sm21, model.st21));
            items.Add(Tuple.Create(model.sm22, model.st22));
            items.Add(Tuple.Create(model.sm23, model.st23));
            items.Add(Tuple.Create(model.sm24, model.st24));
            items.Add(Tuple.Create(model.sm25, model.st25));
            items.Add(Tuple.Create(model.sm26, model.st26));
            items.Add(Tuple.Create(model.sm27, model.st27));
            items.Add(Tuple.Create(model.sm28, model.st28));
            items.Add(Tuple.Create(model.sm29, model.st29));
            items.Add(Tuple.Create(model.sm30, model.st30));
            int ii = 11;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Item1))
                {
                    switch (item.Item1)
                    {
                        case "CIN":
                            model.CinCurrent = item.Item2;
                            break;
                        case "CocId":
                            model.CocIdCurrent = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeCurrent = item.Item2;
                            break;
                        case "ParentCofcId":
                            model.ParentCocIdCurrent = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateCurrent = item.Item2;
                            break;
                        case "CocType":
                            model.CocTypeCurrent = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeCurrent = item.Item2;
                            break;
                        case "CocDispositionInd":
                            model.CocDispositionIndCurrent = item.Item2;
                            break;
                        case "CocExpirationDate":
                            model.ExpirationDateCurrent = item.Item2;
                            break;
                        case "DenialInd":
                            model.DenialIndCurrent = item.Item2;
                            break;
                        case "SubmitterNpi":
                            model.SubmitterNpiCurrent = item.Item2;
                            break;
                        case "ProviderNpi":
                            model.ProviderNpiCurrent = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyCurrent = item.Item2;
                            break;
                        case "MerExemptionId":
                            model.MerExemptionIdCurrent = item.Item2;
                            break;
                        case "ExemptionCode":
                            model.ExemptionCodeCurrent = item.Item2;
                            break;
                        case "ExemptionDate":
                            model.ExemptionDateCurrent = item.Item2;
                            break;
                        case "MerDispositionInd":
                            model.MerDispositionIndCurrent = item.Item2;
                            break;
                        case "MerDispositionDate":
                            model.MerDispositionDateCurrent = item.Item2;
                            break;
                        case "MerCocNotMetInd":
                            model.MerNotMetIndCurrent = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceCurrent = item.Item2;
                            break;
                    }
                }
                switch (ii)
                {
                    case 11:
                        model.sm11Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 12:
                        model.sm12Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 13:
                        model.sm13Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 14:
                        model.sm14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 15:
                        model.sm14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 16:
                        model.sm16Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 17:
                        model.sm17Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 18:
                        model.sm18Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 19:
                        model.sm19Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 20:
                        model.sm20Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 21:
                        model.sm21Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 22:
                        model.sm22Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 23:
                        model.sm23Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 24:
                        model.sm24Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 25:
                        model.sm25Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 26:
                        model.sm26Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 27:
                        model.sm27Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 28:
                        model.sm28Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 29:
                        model.sm29Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 30:
                        model.sm30Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                }
                ii++;
            }
            int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
            model.CocCurrent = await GetCocRecords(0, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
            model.PageCurrent = "1";
            model.PageCurrentTotal = await GetCocPageTotal(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent, pageSizeCurrent);
            model.tbPageCurrent = "1";
            ModelState["PageCurrent"].RawValue = "1";
            ModelState["tbPageCurrent"].RawValue = "1";
            ModelState["PageCurrentTotal"].RawValue = model.PageCurrentTotal;
            model.currentFirstDisabled = true;
            model.currentPreviousDisabled = true;
            if (int.Parse(model.PageCurrentTotal) > 1)
            {
                model.currentNextDisabled = false;
                model.currentLastDisabled = false;
            }
            else
            {
                model.currentNextDisabled = true;
                model.currentLastDisabled = true;
            }
            model.TabActiveCurrent = "active";
            model.TabActiveHistory = "";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:purple;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocCurrent", model.CocCurrent);
            model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
            if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
            model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
            if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();

            GlobalViewModel.PageSizeCurrent = model.PageSizeCurrent;
            return model;
        }
        private async Task<CocViewModel> PageSizeChangeHistory(CocViewModel model)
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();
            items.Add(Tuple.Create(model.hm11, model.ht11));
            items.Add(Tuple.Create(model.hm12, model.ht12));
            items.Add(Tuple.Create(model.hm13, model.ht13));
            items.Add(Tuple.Create(model.hm14, model.ht14));
            items.Add(Tuple.Create(model.hm15, model.ht15));
            items.Add(Tuple.Create(model.hm16, model.ht16));
            items.Add(Tuple.Create(model.hm17, model.ht17));
            items.Add(Tuple.Create(model.hm18, model.ht18));
            items.Add(Tuple.Create(model.hm19, model.ht19));
            items.Add(Tuple.Create(model.hm20, model.ht20));
            items.Add(Tuple.Create(model.hm21, model.ht21));
            items.Add(Tuple.Create(model.hm22, model.ht22));
            items.Add(Tuple.Create(model.hm23, model.ht23));
            items.Add(Tuple.Create(model.hm24, model.ht24));
            items.Add(Tuple.Create(model.hm25, model.ht25));
            items.Add(Tuple.Create(model.hm26, model.ht26));
            items.Add(Tuple.Create(model.hm27, model.ht27));
            items.Add(Tuple.Create(model.hm28, model.ht28));
            items.Add(Tuple.Create(model.hm29, model.ht29));
            items.Add(Tuple.Create(model.hm30, model.ht30));
            int ii = 11;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Item1))
                {
                    switch (item.Item1)
                    {
                        case "CIN":
                            model.CinHistory = item.Item2;
                            break;
                        case "CocId":
                            model.CocIdHistory = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeHistory = item.Item2;
                            break;
                        case "ParentCofcId":
                            model.ParentCocIdHistory = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateHistory = item.Item2;
                            break;
                        case "CocType":
                            model.CocTypeHistory = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeHistory = item.Item2;
                            break;
                        case "CocDispositionInd":
                            model.CocDispositionIndHistory = item.Item2;
                            break;
                        case "CocExpirationDate":
                            model.ExpirationDateHistory = item.Item2;
                            break;
                        case "DenialInd":
                            model.DenialIndHistory = item.Item2;
                            break;
                        case "SubmitterNpi":
                            model.SubmitterNpiHistory = item.Item2;
                            break;
                        case "ProviderNpi":
                            model.ProviderNpiHistory = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyHistory = item.Item2;
                            break;
                        case "MerExemptionId":
                            model.MerExemptionIdHistory = item.Item2;
                            break;
                        case "ExemptionCode":
                            model.ExemptionCodeHistory = item.Item2;
                            break;
                        case "ExemptionDate":
                            model.ExemptionDateHistory = item.Item2;
                            break;
                        case "MerDispositionInd":
                            model.MerDispositionIndHistory = item.Item2;
                            break;
                        case "MerDispositionDate":
                            model.MerDispositionDateHistory = item.Item2;
                            break;
                        case "MerCocNotMetInd":
                            model.MerNotMetIndHistory = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceHistory = item.Item2;
                            break;
                    }
                }
                switch (ii)
                {
                    case 11:
                        model.hm11Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 12:
                        model.hm12Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 13:
                        model.hm13Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 14:
                        model.hm14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 15:
                        model.hm14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 16:
                        model.hm16Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 17:
                        model.hm17Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 18:
                        model.hm18Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 19:
                        model.hm19Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 20:
                        model.hm20Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 21:
                        model.hm21Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 22:
                        model.hm22Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 23:
                        model.hm23Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 24:
                        model.hm24Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 25:
                        model.hm25Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 26:
                        model.hm26Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 27:
                        model.hm27Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 28:
                        model.hm28Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 29:
                        model.hm29Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 30:
                        model.hm30Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                }
                ii++;
            }

            long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
            if (historyHeaderId.HasValue)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.CocHistory = await GetCocRecordsHistory(0, historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetCocHistoryPageTotal(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory, pageSizeHistory);
                model.tbPageHistory = "1";
                ModelState["PageHistory"].RawValue = "1";
                ModelState["tbPageHistory"].RawValue = "1";
                ModelState["PageHistoryTotal"].RawValue = model.PageHistoryTotal;
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                if (int.Parse(model.PageHistoryTotal) > 1)
                {
                    model.historyNextDisabled = false;
                    model.historyLastDisabled = false;
                }
                else
                {
                    model.historyNextDisabled = true;
                    model.historyLastDisabled = true;
                }
            }
            else
            {
                model.CocHistory = new List<McpdContinuityOfCare>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "active";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:purple;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocHistory", model.CocHistory);
            model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
            if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
            model.CocError = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocError");
            if (model.CocError == null) model.CocError = new List<McpdContinuityOfCare>();
            GlobalViewModel.PageSizeHistory = model.PageSizeHistory;
            return model;
        }
        private async Task<CocViewModel> PageSizeChangeError(CocViewModel model)
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();
            items.Add(Tuple.Create(model.em11, model.et11));
            items.Add(Tuple.Create(model.em12, model.et12));
            items.Add(Tuple.Create(model.em13, model.et13));
            items.Add(Tuple.Create(model.em14, model.et14));
            items.Add(Tuple.Create(model.em15, model.et15));
            items.Add(Tuple.Create(model.em16, model.et16));
            items.Add(Tuple.Create(model.em17, model.et17));
            items.Add(Tuple.Create(model.em18, model.et18));
            items.Add(Tuple.Create(model.em19, model.et19));
            items.Add(Tuple.Create(model.em20, model.et20));
            items.Add(Tuple.Create(model.em21, model.et21));
            items.Add(Tuple.Create(model.em22, model.et22));
            items.Add(Tuple.Create(model.em23, model.et23));
            items.Add(Tuple.Create(model.em24, model.et24));
            items.Add(Tuple.Create(model.em25, model.et25));
            items.Add(Tuple.Create(model.em26, model.et26));
            items.Add(Tuple.Create(model.em27, model.et27));
            items.Add(Tuple.Create(model.em28, model.et28));
            items.Add(Tuple.Create(model.em29, model.et29));
            items.Add(Tuple.Create(model.em30, model.et30));
            int ii = 11;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Item1))
                {
                    switch (item.Item1)
                    {
                        case "CIN":
                            model.CinError = item.Item2;
                            break;
                        case "CocId":
                            model.CocIdError = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeError = item.Item2;
                            break;
                        case "ParentCofcId":
                            model.ParentCocIdError = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateError = item.Item2;
                            break;
                        case "CocType":
                            model.CocTypeError = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeError = item.Item2;
                            break;
                        case "CocDispositionInd":
                            model.CocDispositionIndError = item.Item2;
                            break;
                        case "CocExpirationDate":
                            model.ExpirationDateError = item.Item2;
                            break;
                        case "DenialInd":
                            model.DenialIndError = item.Item2;
                            break;
                        case "SubmitterNpi":
                            model.SubmitterNpiError = item.Item2;
                            break;
                        case "ProviderNpi":
                            model.ProviderNpiError = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyError = item.Item2;
                            break;
                        case "MerExemptionId":
                            model.MerExemptionIdError = item.Item2;
                            break;
                        case "ExemptionCode":
                            model.ExemptionCodeError = item.Item2;
                            break;
                        case "ExemptionDate":
                            model.ExemptionDateError = item.Item2;
                            break;
                        case "MerDispositionInd":
                            model.MerDispositionIndError = item.Item2;
                            break;
                        case "MerDispositionDate":
                            model.MerDispositionDateError = item.Item2;
                            break;
                        case "MerCocNotMetInd":
                            model.MerNotMetIndError = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceError = item.Item2;
                            break;
                    }
                }
                switch (ii)
                {
                    case 11:
                        model.em11Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 12:
                        model.em12Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 13:
                        model.em13Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 14:
                        model.em14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 15:
                        model.em14Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 16:
                        model.em16Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 17:
                        model.em17Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 18:
                        model.em18Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 19:
                        model.em19Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 20:
                        model.em20Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 21:
                        model.em21Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 22:
                        model.em22Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 23:
                        model.em23Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 24:
                        model.em24Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 25:
                        model.em25Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 26:
                        model.em26Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 27:
                        model.em27Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 28:
                        model.em28Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 29:
                        model.em29Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                    case 30:
                        model.em30Style = string.IsNullOrEmpty(item.Item1) ? "display:none;" : "display:block;";
                        break;
                }
                ii++;
            }

            long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
            if (errorHeaderId.HasValue)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.CocError = await GetCocRecordsError(0, errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetCocErrorPageTotal(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError, pageSizeError);
                model.tbPageError = "1";
                ModelState["PageError"].RawValue = "1";
                ModelState["tbPageError"].RawValue = "1";
                ModelState["PageErrorTotal"].RawValue = model.PageErrorTotal;
                model.errorFirstDisabled = true;
                model.errorPreviousDisabled = true;
                if (int.Parse(model.PageErrorTotal) > 1)
                {
                    model.errorNextDisabled = false;
                    model.errorLastDisabled = false;
                }
                else
                {
                    model.errorNextDisabled = true;
                    model.errorLastDisabled = true;
                }
            }
            else
            {
                model.CocError = new List<McpdContinuityOfCare>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "";
            model.TabActiveError = "active";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:purple;";
            HttpContext.Session.Set<List<McpdContinuityOfCare>>("CocError", model.CocError);
            model.CocCurrent = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocCurrent");
            if (model.CocCurrent == null) model.CocCurrent = new List<McpdContinuityOfCare>();
            model.CocHistory = HttpContext.Session.Get<List<McpdContinuityOfCare>>("CocHistory");
            if (model.CocHistory == null) model.CocHistory = new List<McpdContinuityOfCare>();
            GlobalViewModel.PageSizeError = model.PageSizeError;
            return model;
        }
        public async Task<IActionResult> DownloadFile(long? id, CocViewModel model)
        {
            List<McpdContinuityOfCare> Cocs = new List<McpdContinuityOfCare>();
            string CocType = "";
            string exportType = "";
            if (id == 4)
            {
                //download current
                Cocs = await GetCocForDownload(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.CocIdCurrent, model.RecordTypeCurrent, model.ParentCocIdCurrent, model.ReceiveDateCurrent, model.CocTypeCurrent, model.BenefitTypeCurrent, model.CocDispositionIndCurrent, model.ExpirationDateCurrent, model.DenialIndCurrent, model.SubmitterNpiCurrent, model.ProviderNpiCurrent, model.ProviderTaxonomyCurrent, model.MerExemptionIdCurrent, model.ExemptionCodeCurrent, model.ExemptionDateCurrent, model.MerDispositionIndCurrent, model.MerDispositionDateCurrent, model.MerNotMetIndCurrent, model.DataSourceCurrent);
                CocType = "Coc_staging";
                exportType = model.selectedExport;
            }
            else if (id == 5)
            {
                //download history
                long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
                if (historyHeaderId.HasValue)
                {
                    Cocs = await GetCocHistoryForDownload(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.CocIdHistory, model.RecordTypeHistory, model.ParentCocIdHistory, model.ReceiveDateHistory, model.CocTypeHistory, model.BenefitTypeHistory, model.CocDispositionIndHistory, model.ExpirationDateHistory, model.DenialIndHistory, model.SubmitterNpiHistory, model.ProviderNpiHistory, model.ProviderTaxonomyHistory, model.MerExemptionIdHistory, model.ExemptionCodeHistory, model.ExemptionDateHistory, model.MerDispositionIndHistory, model.MerDispositionDateHistory, model.MerNotMetIndHistory, model.DataSourceHistory);
                }
                CocType = "Coc_history";
                exportType = model.selectedExportHistory;
            }
            else if (id == 6)
            {
                //download error
                long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
                if (errorHeaderId.HasValue)
                {
                    Cocs = await GetCocErrorForDownload(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.CocIdError, model.RecordTypeError, model.ParentCocIdError, model.ReceiveDateError, model.CocTypeError, model.BenefitTypeError, model.CocDispositionIndError, model.ExpirationDateError, model.DenialIndError, model.SubmitterNpiError, model.ProviderNpiError, model.ProviderTaxonomyError, model.MerExemptionIdError, model.ExemptionCodeError, model.ExemptionDateError, model.MerDispositionIndError, model.MerDispositionDateError, model.MerNotMetIndError, model.DataSourceError);
                }
                CocType = "Coc_error";
                exportType = model.selectedExportError;
            }
            if (exportType == ".csv")
            {
                var columnHeader = new string[] { "McpdHeaderId", "PlanCode", "Cin", "CocId", "RecordType", "ParentCocId", "CocReceivedDate", "CocType", "BenefitType", "CocDispositionIndicator", "CocExpirationDate", "CocDenialReasonIndicator", "SubmittingProviderNpi", "CocProviderNpi", "ProviderTaxonomy", "MerExemptionId", "ExemptionToEnrollmentDenialCode", "ExemptionToEnrollmentDenialDate", "MerCocDispositionIndicator", "ReasonMerCocNotMetIndicator", "TradingPartnerCode", "ErrorMessage", "DataSource" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(",", columnHeader));
                Cocs.ForEach(x => sb.AppendLine($"{x.McpdHeaderId.ToString()},{x.PlanCode },{x.Cin },{x.CocId},{x.RecordType},{x.ParentCocId},{x.CocReceivedDate},{x.CocType},{x.BenefitType},{x.CocDispositionIndicator},{x.CocExpirationDate},{x.CocDenialReasonIndicator},{x.SubmittingProviderNpi},{x.CocProviderNpi},{x.ProviderTaxonomy},{x.MerExemptionId},{x.ExemptionToEnrollmentDenialCode},{x.ExemptionToEnrollmentDenialDate},{x.MerCocDispositionIndicator},{x.MerCocDispositionDate},{x.ReasonMerCocNotMetIndicator},{x.TradingPartnerCode},{x.ErrorMessage},{x.DataSource}"));
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/csv", CocType + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            }
            else if(exportType=="json")
            {
                McpdHeader mcpdHeader = _context.McpdHeaders.Find(Cocs[0].McpdHeaderId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{\"header\":");
                sb.Append(JsonOperations.GetMcpdHeaderJson(mcpdHeader));
                sb.Append($",\"{CocType}\":");
                sb.Append(JsonOperations.GetCocJson(Cocs));
                sb.Append("}");
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/json", CocType + DateTime.Today.ToString("yyyyMMdd") + ".json");
            }
            else
            {
                string fileName = CocType + DateTime.Today.ToString("yyyyMMdd") + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(Cocs.ToDataTable());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
        }
        // GET: Coc/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdContinuityOfCare = await _context.McpdContinuityOfCare
                .FirstOrDefaultAsync(m => m.McpdContinuityOfCareId == id);
            if (mcpdContinuityOfCare == null)
            {
                return NotFound();
            }

            return View(mcpdContinuityOfCare);
        }

        // GET: Coc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coc/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("McpdContinuityOfCareId,McpdHeaderId,PlanCode,Cin,CocId,RecordType,ParentCocId,CocReceivedDate,CocType,BenefitType,CocDispositionIndicator,CocExpirationDate,CocDenialReasonIndicator,SubmittingProviderNpi,CocProviderNpi,ProviderTaxonomy,MerExemptionId,ExemptionToEnrollmentDenialCode,ExemptionToEnrollmentDenialDate,MerCocDispositionIndicator,MerCocDispositionDate,ReasonMerCocNotMetIndicator")] McpdContinuityOfCare mcpdContinuityOfCare)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mcpdContinuityOfCare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdContinuityOfCare);
        }

        // GET: Coc/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdContinuityOfCare = await _context.McpdContinuityOfCare.FindAsync(id);
            if (mcpdContinuityOfCare == null)
            {
                return NotFound();
            }
            return View(mcpdContinuityOfCare);
        }

        // POST: Coc/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("McpdContinuityOfCareId,McpdHeaderId,PlanCode,Cin,CocId,RecordType,ParentCocId,CocReceivedDate,CocType,BenefitType,CocDispositionIndicator,CocExpirationDate,CocDenialReasonIndicator,SubmittingProviderNpi,CocProviderNpi,ProviderTaxonomy,MerExemptionId,ExemptionToEnrollmentDenialCode,ExemptionToEnrollmentDenialDate,MerCocDispositionIndicator,MerCocDispositionDate,ReasonMerCocNotMetIndicator,TradingPartnerCode,DataSource")] McpdContinuityOfCare mcpdContinuityOfCare)
        {
            if (id != mcpdContinuityOfCare.McpdContinuityOfCareId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mcpdContinuityOfCare);
                    await _context.SaveChangesAsync();
                    OperationLog logOperation = new OperationLog
                    {
                        Message = "Edit " + mcpdContinuityOfCare.CocId,
                        ModuleName = "COC Edit",
                        OperationTime = DateTime.Now,
                        UserId = User.Identity.Name
                    };
                    _contextLog.OperationLogs.Add(logOperation);
                    await _contextLog.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!McpdContinuityOfCareExists(mcpdContinuityOfCare.McpdContinuityOfCareId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdContinuityOfCare);
        }

        // GET: Coc/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdContinuityOfCare = await _context.McpdContinuityOfCare
                .FirstOrDefaultAsync(m => m.McpdContinuityOfCareId == id);
            if (mcpdContinuityOfCare == null)
            {
                return NotFound();
            }

            return View(mcpdContinuityOfCare);
        }

        // POST: Coc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var mcpdContinuityOfCare = await _context.McpdContinuityOfCare.FindAsync(id);
            _context.McpdContinuityOfCare.Remove(mcpdContinuityOfCare);
            await _context.SaveChangesAsync();
            OperationLog logOperation = new OperationLog
            {
                Message = "Delete " + mcpdContinuityOfCare.CocId,
                ModuleName = "COC Delete",
                OperationTime = DateTime.Now,
                UserId = User.Identity.Name
            };
            _contextLog.OperationLogs.Add(logOperation);
            await _contextLog.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool McpdContinuityOfCareExists(long id)
        {
            return _context.McpdContinuityOfCare.Any(e => e.McpdContinuityOfCareId == id);
        }
    }
}

