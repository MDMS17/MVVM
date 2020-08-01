using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;
using mcpdandpcpa.Extensions;
using JsonLib;
using System.Collections.Generic;
using System;

namespace mcpdandpcpa.Controllers
{
    public class OonController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public OonController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }
        public async Task<IActionResult> Index()
        {
            OonViewModel model = new OonViewModel();
            model.OonCurrent = await _context.McpdOutOfNetwork.Take(int.Parse(model.PageSizeCurrent)).ToListAsync();
            model.PageCurrent = "1";
            model.PageCurrentTotal = Math.Ceiling((decimal)_context.McpdOutOfNetwork.Count() / int.Parse(model.PageSizeCurrent)).ToString();
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
                model.OonHistory = await GetOonRecordsHistory(0, historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetOonHistoryPageTotal(historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.tbPageHistory = "1";
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                model.historyNextDisabled = false;
                model.historyLastDisabled = false;
            }
            else
            {
                model.OonHistory = new List<McpdOutOfNetwork>();
            }
            var errorHeader = await _contextError.McpdHeaders.OrderByDescending(x => x.McpdHeaderId).FirstOrDefaultAsync();
            if (errorHeader != null)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.selectedYearError = errorHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthError = errorHeader.ReportingPeriod.Substring(4, 2);
                model.OonError = await GetOonRecordsError(0, errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetOonErrorPageTotal(errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
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
                model.OonError = new List<McpdOutOfNetwork>();
            }
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? id, OonViewModel model)
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
                    model.OonCurrent = await GetOonRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonCurrent = await GetOonRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonCurrent = await GetOonRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonCurrent = await GetOonRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonCurrent = await GetOonRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonHistory = await GetOonRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonHistory = await GetOonRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonHistory = await GetOonRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonHistory = await GetOonRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonHistory = await GetOonRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
                    if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
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
                    model.OonError = await GetOonRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
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
                    model.OonError = await GetOonRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
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
                    model.OonError = await GetOonRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
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
                    model.OonError = await GetOonRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
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
                    model.OonError = await GetOonRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
                    model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
                    if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
                    model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
                    if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
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

        private async Task<List<McpdOutOfNetwork>> GetOonRecords(int pageCurrent, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            var result = _context.McpdOutOfNetwork.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Skip(pageCurrent * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetOonPageTotal(string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_context.McpdOutOfNetwork.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdOutOfNetwork>> GetOonRecordsHistory(int pageNumber, long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            var result = _contextHistory.McpdOutOfNetwork.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
               .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetOonHistoryPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextHistory.McpdOutOfNetwork.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdOutOfNetwork>> GetOonRecordsError(int pageNumber, long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            var result = _contextError.McpdOutOfNetwork.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetOonErrorPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextError.McpdOutOfNetwork.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdOutOfNetwork>> GetOonForDownload(string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource)
        {
            var result = _context.McpdOutOfNetwork.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdOutOfNetwork>> GetOonHistoryForDownload(long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource)
        {
            var result = _contextHistory.McpdOutOfNetwork.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdOutOfNetwork>> GetOonErrorForDownload(long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string OonId, string RecordType, string ParentOonId, string ReceiveDate, string ReferralInd, string StatusInd, string ResolveDate, string ApprovalExplain, string SpecialistNpi, string ProviderTaxonomy, string AddressLine1, string AddressLine2, string AddressCity, string AddressState, string AddressZip, string AddressCountry, string DataSource)
        {
            var result = _contextError.McpdOutOfNetwork.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByOonId(OonId).FilterByOonRecordType(RecordType).FilterByParentOonId(ParentOonId).FilterByOonReceiveDate(ReceiveDate)
                .FilterByReferralInd(ReferralInd).FilterByOonStatusInd(StatusInd).FilterByResolveDate(ResolveDate)
                .FilterByApprovalExplain(ApprovalExplain).FilterBySpecialistNpi(SpecialistNpi).FilterByOonProviderTaxonomy(ProviderTaxonomy)
                .FilterByAddressLine1(AddressLine1).FilterByAddressLine2(AddressLine2).FilterByAddressCity(AddressCity)
                .FilterByAddressState(AddressState).FilterByAddressZip(AddressZip).FilterByAddressCountry(AddressCountry)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<OonViewModel> PageSizeChangeCurrent(OonViewModel model)
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
                        case "OonId":
                            model.OonIdCurrent = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeCurrent = item.Item2;
                            break;
                        case "ParentOofnId":
                            model.ParentOonIdCurrent = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateCurrent = item.Item2;
                            break;
                        case "ReferralInd":
                            model.ReferralIndCurrent = item.Item2;
                            break;
                        case "StatusInd":
                            model.StatusIndCurrent = item.Item2;
                            break;
                        case "ResolveDate":
                            model.ResolveDateCurrent = item.Item2;
                            break;
                        case "ApprovalExplain":
                            model.ApprovalExplainCurrent = item.Item2;
                            break;
                        case "SpecialistNpi":
                            model.SpecialistNpiCurrent = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyCurrent = item.Item2;
                            break;
                        case "AddressLine1":
                            model.AddressLine1Current = item.Item2;
                            break;
                        case "AddressLine2":
                            model.AddressLine2Current = item.Item2;
                            break;
                        case "AddressCity":
                            model.AddressCityCurrent = item.Item2;
                            break;
                        case "AddressState":
                            model.AddressStateCurrent = item.Item2;
                            break;
                        case "AddressZip":
                            model.AddressZipCurrent = item.Item2;
                            break;
                        case "AddressCountry":
                            model.AddressCountryCurrent = item.Item2;
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
                }
                ii++;
            }
            int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
            model.OonCurrent = await GetOonRecords(0, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
            model.PageCurrent = "1";
            model.PageCurrentTotal = await GetOonPageTotal(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonCurrent", model.OonCurrent);
            model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
            if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
            model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
            if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();

            GlobalViewModel.PageSizeCurrent = model.PageSizeCurrent;
            return model;
        }
        private async Task<OonViewModel> PageSizeChangeHistory(OonViewModel model)
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
                        case "OonId":
                            model.OonIdHistory = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeHistory = item.Item2;
                            break;
                        case "ParentCofcId":
                            model.ParentOonIdHistory = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateHistory = item.Item2;
                            break;
                        case "ReferralInd":
                            model.ReferralIndHistory = item.Item2;
                            break;
                        case "StatusInd":
                            model.StatusIndHistory = item.Item2;
                            break;
                        case "ResolveDate":
                            model.ResolveDateHistory = item.Item2;
                            break;
                        case "ApprovalExplain":
                            model.ApprovalExplainHistory = item.Item2;
                            break;
                        case "SpecialistNpi":
                            model.SpecialistNpiHistory = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyHistory = item.Item2;
                            break;
                        case "AddressLine1":
                            model.AddressLine1History = item.Item2;
                            break;
                        case "AddressLine2":
                            model.AddressLine2History = item.Item2;
                            break;
                        case "AddressCity":
                            model.AddressCityHistory = item.Item2;
                            break;
                        case "AddressState":
                            model.AddressStateHistory = item.Item2;
                            break;
                        case "AddressZip":
                            model.AddressZipHistory = item.Item2;
                            break;
                        case "AddressCountry":
                            model.AddressCountryHistory = item.Item2;
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
                }
                ii++;
            }

            long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
            if (historyHeaderId.HasValue)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.OonHistory = await GetOonRecordsHistory(0, historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetOonHistoryPageTotal(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory, pageSizeHistory);
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
                model.OonHistory = new List<McpdOutOfNetwork>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "active";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:purple;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonHistory", model.OonHistory);
            model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
            if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
            model.OonError = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonError");
            if (model.OonError == null) model.OonError = new List<McpdOutOfNetwork>();
            GlobalViewModel.PageSizeHistory = model.PageSizeHistory;
            return model;
        }
        private async Task<OonViewModel> PageSizeChangeError(OonViewModel model)
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
                        case "OonId":
                            model.OonIdError = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeError = item.Item2;
                            break;
                        case "ParentCofcId":
                            model.ParentOonIdError = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateError = item.Item2;
                            break;
                        case "ReferralInd":
                            model.ReferralIndError = item.Item2;
                            break;
                        case "StatusInd":
                            model.StatusIndError = item.Item2;
                            break;
                        case "ResolveDate":
                            model.ResolveDateError = item.Item2;
                            break;
                        case "ApprovalExplain":
                            model.ApprovalExplainError = item.Item2;
                            break;
                        case "SpecialistNpi":
                            model.SpecialistNpiError = item.Item2;
                            break;
                        case "ProviderTaxonomy":
                            model.ProviderTaxonomyError = item.Item2;
                            break;
                        case "AddressLine1":
                            model.AddressLine1Error = item.Item2;
                            break;
                        case "AddressLine2":
                            model.AddressLine2Error = item.Item2;
                            break;
                        case "AddressCity":
                            model.AddressCityError = item.Item2;
                            break;
                        case "AddressState":
                            model.AddressStateError = item.Item2;
                            break;
                        case "AddressZip":
                            model.AddressZipError = item.Item2;
                            break;
                        case "AddressCountry":
                            model.AddressCountryError = item.Item2;
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
                }
                ii++;
            }

            long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
            if (errorHeaderId.HasValue)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.OonError = await GetOonRecordsError(0, errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetOonErrorPageTotal(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError, pageSizeError);
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
                model.OonError = new List<McpdOutOfNetwork>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "";
            model.TabActiveError = "active";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:purple;";
            HttpContext.Session.Set<List<McpdOutOfNetwork>>("OonError", model.OonError);
            model.OonCurrent = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonCurrent");
            if (model.OonCurrent == null) model.OonCurrent = new List<McpdOutOfNetwork>();
            model.OonHistory = HttpContext.Session.Get<List<McpdOutOfNetwork>>("OonHistory");
            if (model.OonHistory == null) model.OonHistory = new List<McpdOutOfNetwork>();
            GlobalViewModel.PageSizeError = model.PageSizeError;
            return model;
        }
        public async Task<IActionResult> DownloadFile(long? id, OonViewModel model)
        {
            List<McpdOutOfNetwork> Oons = new List<McpdOutOfNetwork>();
            string OonType = "";
            string exportType = "";
            if (id == 4)
            {
                //download current
                Oons = await GetOonForDownload(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.OonIdCurrent, model.RecordTypeCurrent, model.ParentOonIdCurrent, model.ReceiveDateCurrent, model.ReferralIndCurrent, model.StatusIndCurrent, model.ResolveDateCurrent, model.ApprovalExplainCurrent, model.SpecialistNpiCurrent, model.ProviderTaxonomyCurrent, model.AddressLine1Current, model.AddressLine2Current, model.AddressCityCurrent, model.AddressStateCurrent, model.AddressZipCurrent, model.AddressCountryCurrent, model.DataSourceCurrent);
                OonType = "Oon_staging";
                exportType = model.selectedExport;
            }
            else if (id == 5)
            {
                //download history
                long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
                if (historyHeaderId.HasValue)
                {
                    Oons = await GetOonHistoryForDownload(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.OonIdHistory, model.RecordTypeHistory, model.ParentOonIdHistory, model.ReceiveDateHistory, model.ReferralIndHistory, model.StatusIndHistory, model.ResolveDateHistory, model.ApprovalExplainHistory, model.SpecialistNpiHistory, model.ProviderTaxonomyHistory, model.AddressLine1History, model.AddressLine2History, model.AddressCityHistory, model.AddressStateHistory, model.AddressZipHistory, model.AddressCountryHistory, model.DataSourceHistory);
                }
                OonType = "Oon_history";
                exportType = model.selectedExportHistory;
            }
            else if (id == 6)
            {
                //download error
                long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
                if (errorHeaderId.HasValue)
                {
                    Oons = await GetOonErrorForDownload(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.OonIdError, model.RecordTypeError, model.ParentOonIdError, model.ReceiveDateError, model.ReferralIndError, model.StatusIndError, model.ResolveDateError, model.ApprovalExplainError, model.SpecialistNpiError, model.ProviderTaxonomyError, model.AddressLine1Error, model.AddressLine2Error, model.AddressCityError, model.AddressStateError, model.AddressZipError, model.AddressCountryError, model.DataSourceError);
                }
                OonType = "Oon_error";
                exportType = model.selectedExportError;
            }
            if (exportType == ".csv")
            {
                var columnHeader = new string[] { "McpdHeaderId", "PlanCode", "Cin", "OonId", "RecordType", "ParentOonId", "OonRequestReceivedDate", "ReferralRequestReasonIndicator", "OonResolutionStatusIndicator", "OonRequestResolvedDate", "PartialApprovalExplanation", "SpecialistProviderNpi", "ProviderTaxonomy", "ServiceLocationAddressLine1", "ServiceLocationAddressLine2", "ServiceLocationCity", "ServiceLocationState", "ServiceLocationZip", "ServiceLocationCountry", "TradingPartnerCode", "ErrorMessage", "DataSource" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(",", columnHeader));
                Oons.ForEach(x => sb.AppendLine($"{x.McpdHeaderId.ToString()},{x.PlanCode },{x.Cin },{x.OonId},{x.RecordType},{x.ParentOonId},{x.OonRequestReceivedDate},{x.ReferralRequestReasonIndicator},{x.OonResolutionStatusIndicator},{x.OonRequestResolvedDate},{x.PartialApprovalExplanation},{x.SpecialistProviderNpi},{x.ProviderTaxonomy},{x.ServiceLocationAddressLine1},{x.ServiceLocationAddressLine2 },{x.ServiceLocationCity },{x.ServiceLocationState },{x.ServiceLocationZip },{x.ServiceLocationCountry},{x.TradingPartnerCode},{x.ErrorMessage},{x.DataSource}"));
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/csv", OonType + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            }
            else
            {
                McpdHeader mcpdHeader = _context.McpdHeaders.Find(Oons[0].McpdHeaderId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{\"header\":");
                sb.Append(JsonOperations.GetMcpdHeaderJson(mcpdHeader));
                sb.Append($",\"{OonType}\":");
                sb.Append(JsonOperations.GetOonJson(Oons));
                sb.Append("}");
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/json", OonType + DateTime.Today.ToString("yyyyMMdd") + ".json");
            }
        }
        // GET: Oon/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdOutOfNetwork = await _context.McpdOutOfNetwork
                .FirstOrDefaultAsync(m => m.McpdOutOfNetworkId == id);
            if (mcpdOutOfNetwork == null)
            {
                return NotFound();
            }

            return View(mcpdOutOfNetwork);
        }

        // GET: Oon/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Oon/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("McpdOutOfNetworkId,McpdHeaderId,PlanCode,Cin,OonId,RecordType,ParentOonId,OonRequestReceivedDate,OonRequestResolvedDate,PartialApprovalExplanation,SpecialistProviderNpi,ProviderTaxonomy,ServiceLocationAddressLine1,ServiceLocationAddressLine2,ServiceLocationCity,ServiceLocationState,ServiceLocationZip,ServiceLocationCountry")] McpdOutOfNetwork mcpdOutOfNetwork)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mcpdOutOfNetwork);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdOutOfNetwork);
        }

        // GET: Oon/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdOutOfNetwork = await _context.McpdOutOfNetwork.FindAsync(id);
            if (mcpdOutOfNetwork == null)
            {
                return NotFound();
            }
            return View(mcpdOutOfNetwork);
        }

        // POST: Oon/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("McpdOutOfNetworkId,McpdHeaderId,PlanCode,Cin,OonId,RecordType,ParentOonId,OonRequestReceivedDate,OonRequestResolvedDate,PartialApprovalExplanation,SpecialistProviderNpi,ProviderTaxonomy,ServiceLocationAddressLine1,ServiceLocationAddressLine2,ServiceLocationCity,ServiceLocationState,ServiceLocationZip,ServiceLocationCountry")] McpdOutOfNetwork mcpdOutOfNetwork)
        {
            if (id != mcpdOutOfNetwork.McpdOutOfNetworkId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mcpdOutOfNetwork);
                    await _context.SaveChangesAsync();
                    OperationLog logOperation = new OperationLog
                    {
                        Message = "Edit " + mcpdOutOfNetwork.OonId,
                        ModuleName = "OON Edit",
                        OperationTime = DateTime.Now,
                        UserId = User.Identity.Name
                    };
                    _contextLog.OperationLogs.Add(logOperation);
                    await _contextLog.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!McpdOutOfNetworkExists(mcpdOutOfNetwork.McpdOutOfNetworkId))
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
            return View(mcpdOutOfNetwork);
        }

        // GET: Oon/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdOutOfNetwork = await _context.McpdOutOfNetwork
                .FirstOrDefaultAsync(m => m.McpdOutOfNetworkId == id);
            if (mcpdOutOfNetwork == null)
            {
                return NotFound();
            }

            return View(mcpdOutOfNetwork);
        }

        // POST: Oon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var mcpdOutOfNetwork = await _context.McpdOutOfNetwork.FindAsync(id);
            _context.McpdOutOfNetwork.Remove(mcpdOutOfNetwork);
            await _context.SaveChangesAsync();
            OperationLog logOperation = new OperationLog
            {
                Message = "Delete " + mcpdOutOfNetwork.OonId,
                ModuleName = "OON Delete",
                OperationTime = DateTime.Now,
                UserId = User.Identity.Name
            };
            _contextLog.OperationLogs.Add(logOperation);
            await _contextLog.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool McpdOutOfNetworkExists(long id)
        {
            return _context.McpdOutOfNetwork.Any(e => e.McpdOutOfNetworkId == id);
        }
    }
}
