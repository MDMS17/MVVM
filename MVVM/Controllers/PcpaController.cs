using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;
using System.IO;
using mcpdandpcpa.Extensions;
using JsonLib;
using ClosedXML.Excel;

namespace mcpdandpcpa.Controllers
{
    public class PcpaController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public PcpaController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }

        // GET: Pcpa
        public async Task<IActionResult> Index()
        {
            PcpaViewModel model = new PcpaViewModel();
            model.PcpaCurrent = await _context.PcpAssignments.Take(int.Parse(model.PageSizeCurrent)).ToListAsync();
            model.PageCurrent = "1";
            model.PageCurrentTotal = Math.Ceiling((decimal)_context.PcpAssignments.Count() / int.Parse(model.PageSizeCurrent)).ToString();
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
            var historyHeader = await _contextHistory.PcpHeaders.OrderByDescending(x => x.PcpHeaderId).FirstOrDefaultAsync();
            if (historyHeader != null)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.selectedYearHistory = historyHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthHistory = historyHeader.ReportingPeriod.Substring(4, 2);
                model.PcpaHistory = await GetPcpaRecordsHistory(0, historyHeader.PcpHeaderId, "All", "All", "", "", "", pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetPcpaHistoryPageTotal(historyHeader.PcpHeaderId, "All", "All", "", "", "", pageSizeHistory);
                model.tbPageHistory = "1";
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                model.historyNextDisabled = false;
                model.historyLastDisabled = false;
            }
            else
            {
                model.PcpaHistory = new List<PcpAssignment>();
            }
            var errorHeader = await _contextError.PcpHeaders.OrderByDescending(x => x.PcpHeaderId).FirstOrDefaultAsync();
            if (errorHeader != null)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.selectedYearError = errorHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthError = errorHeader.ReportingPeriod.Substring(4, 2);
                model.PcpaError = await GetPcpaRecordsError(0, errorHeader.PcpHeaderId, "All", "All", "", "", "", pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetPcpaErrorPageTotal(errorHeader.PcpHeaderId, "All", "All", "", "", "", pageSizeError);
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
                model.PcpaError = new List<PcpAssignment>();
            }
            HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
            HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
            HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? id, PcpaViewModel model)
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
                    model.PcpaCurrent = await GetPcpaRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    model.PcpaCurrent = await GetPcpaRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    model.PcpaCurrent = await GetPcpaRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    model.PcpaCurrent = await GetPcpaRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    model.PcpaCurrent = await GetPcpaRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaCurrent", model.PcpaCurrent);
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).PcpHeaderId;
                    model.PcpaHistory = await GetPcpaRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).PcpHeaderId;
                    model.PcpaHistory = await GetPcpaRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).PcpHeaderId;
                    model.PcpaHistory = await GetPcpaRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).PcpHeaderId;
                    model.PcpaHistory = await GetPcpaRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory).PcpHeaderId;
                    model.PcpaHistory = await GetPcpaRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
                    if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
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
                    long errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).PcpHeaderId;
                    model.PcpaError = await GetPcpaRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
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
                    long errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).PcpHeaderId;
                    model.PcpaError = await GetPcpaRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
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
                    long errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).PcpHeaderId;
                    model.PcpaError = await GetPcpaRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
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
                    long errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).PcpHeaderId;
                    model.PcpaError = await GetPcpaRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
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
                    long errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError).PcpHeaderId;
                    model.PcpaError = await GetPcpaRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
                    model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
                    if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
                    model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
                    if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
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

        private async Task<List<PcpAssignment>> GetPcpaRecords(int pageCurrent, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            var result = _context.PcpAssignments.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Skip(pageCurrent * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetPcpaPageTotal(string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_context.PcpAssignments.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode<PcpAssignment>(PlanCode).FilterByCin<PcpAssignment>(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<PcpAssignment>> GetPcpaRecordsHistory(int pageNumber, long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            var result = _contextHistory.PcpAssignments.Where(x => x.PcpHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode<PcpAssignment>(PlanCode).FilterByCin<PcpAssignment>(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetPcpaHistoryPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextHistory.PcpAssignments.Where(x => x.PcpHeaderId == headerId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode<PcpAssignment>(PlanCode).FilterByCin<PcpAssignment>(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<PcpAssignment>> GetPcpaRecordsError(int pageNumber, long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            var result = _contextError.PcpAssignments.Where(x => x.PcpHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode<PcpAssignment>(PlanCode).FilterByCin<PcpAssignment>(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetPcpaErrorPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextError.PcpAssignments.Where(x => x.PcpHeaderId == headerId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode<PcpAssignment>(PlanCode).FilterByCin<PcpAssignment>(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource).Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<PcpAssignment>> GetPcpaForDownload(string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource)
        {
            var result = _context.PcpAssignments.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<PcpAssignment>> GetPcpaHistoryForDownload(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource)
        {
            var result = _contextHistory.PcpAssignments.Where(x => x.PcpHeaderId == headerId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<PcpAssignment>> GetPcpaErrorForDownload(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string Npi, string DataSource)
        {
            var result = _contextError.PcpAssignments.Where(x => x.PcpHeaderId == headerId).FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByNpi(Npi).FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<PcpaViewModel> PageSizeChangeCurrent(PcpaViewModel model)
        {
            int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
            model.PcpaCurrent = await GetPcpaRecords(0, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
            model.PageCurrent = "1";
            model.PageCurrentTotal = await GetPcpaPageTotal(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
            HttpContext.Session.Set("GrievanceCurrent", model.PcpaCurrent);
            model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
            if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
            model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
            if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();

            GlobalViewModel.PageSizeCurrent = model.PageSizeCurrent;
            return model;
        }
        private async Task<PcpaViewModel> PageSizeChangeHistory(PcpaViewModel model)
        {
            long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
            if (historyHeaderId.HasValue)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.PcpaHistory = await GetPcpaRecordsHistory(0, historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetPcpaHistoryPageTotal(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory, pageSizeHistory);
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
                model.PcpaHistory = new List<PcpAssignment>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "active";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:purple;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<PcpAssignment>>("PcpaHistory", model.PcpaHistory);
            model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
            if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
            model.PcpaError = HttpContext.Session.Get<List<PcpAssignment>>("PcpaError");
            if (model.PcpaError == null) model.PcpaError = new List<PcpAssignment>();
            GlobalViewModel.PageSizeHistory = model.PageSizeHistory;
            return model;
        }
        private async Task<PcpaViewModel> PageSizeChangeError(PcpaViewModel model)
        {
            long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
            if (errorHeaderId.HasValue)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.PcpaError = await GetPcpaRecordsError(0, errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetPcpaErrorPageTotal(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError, pageSizeError);
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
                model.PcpaError = new List<PcpAssignment>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "";
            model.TabActiveError = "active";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:purple;";
            HttpContext.Session.Set<List<PcpAssignment>>("PcpaError", model.PcpaError);
            model.PcpaCurrent = HttpContext.Session.Get<List<PcpAssignment>>("PcpaCurrent");
            if (model.PcpaCurrent == null) model.PcpaCurrent = new List<PcpAssignment>();
            model.PcpaHistory = HttpContext.Session.Get<List<PcpAssignment>>("PcpaHistory");
            if (model.PcpaHistory == null) model.PcpaHistory = new List<PcpAssignment>();
            GlobalViewModel.PageSizeError = model.PageSizeError;
            return model;
        }
        public async Task<IActionResult> DownloadFile(long? id, PcpaViewModel model)
        {
            List<PcpAssignment> PcpaAssignments = new List<PcpAssignment>();
            string pcpaType = "";
            string exportType = "";
            if (id == 4)
            {
                //download current
                PcpaAssignments = await GetPcpaForDownload(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.NpiCurrent, model.DataSourceCurrent);
                pcpaType = "pcpa_staging";
                exportType = model.selectedExport;
            }
            else if (id == 5)
            {
                //download history
                long? historyHeaderId = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.PcpHeaderId;
                if (historyHeaderId.HasValue)
                {
                    PcpaAssignments = await GetPcpaHistoryForDownload(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.NpiHistory, model.DataSourceHistory);
                }
                pcpaType = "pcpa_history";
                exportType = model.selectedExportHistory;
            }
            else if (id == 6)
            {
                //download error
                long? errorHeaderId = _contextError.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.PcpHeaderId;
                if (errorHeaderId.HasValue)
                {
                    PcpaAssignments = await GetPcpaErrorForDownload(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.NpiError, model.DataSourceError);
                }
                pcpaType = "pcpa_error";
                exportType = model.selectedExportError;
            }
            if (exportType == ".csv")
            {
                var columnHeader = new string[] { "PcpHeaderId", "PlanCode", "Cin", "Npi", "TradingPartnerCode", "ErrorMessage", "DataSource" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(",", columnHeader));
                PcpaAssignments.ForEach(x => sb.AppendLine($"{x.PcpHeaderId.ToString()},{x.PlanCode },{x.Cin },{x.Npi},{x.TradingPartnerCode},{x.ErrorMessage},{x.DataSource}"));
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/csv", pcpaType + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            }
            else if(exportType=="json")
            {
                PcpHeader pcpHeader = _context.PcpHeaders.Find(PcpaAssignments[0].PcpHeaderId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{\"header\":");
                sb.Append(JsonOperations.GetPcpaHeaderJson(pcpHeader));
                sb.Append($",\"{pcpaType}\":");
                sb.Append(JsonOperations.GetPcpaDetailJson(PcpaAssignments));
                sb.Append("}");
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/json", pcpaType + DateTime.Today.ToString("yyyyMMdd") + ".json");
            }
            else
            {
                string fileName = pcpaType + DateTime.Today.ToString("yyyyMMdd") + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(PcpaAssignments.ToDataTable());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }

            }
        }
        // GET: Pcpa/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcpAssignment = await _context.PcpAssignments
                .FirstOrDefaultAsync(m => m.PcpAssignmentId == id);
            if (pcpAssignment == null)
            {
                return NotFound();
            }

            return View(pcpAssignment);
        }

        // GET: Pcpa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pcpa/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PcpAssignmentId,PcpHeaderId,PlanCode,Cin,Npi,TradingPartnerCode")] PcpAssignment pcpAssignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pcpAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pcpAssignment);
        }

        // GET: Pcpa/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcpAssignment = await _context.PcpAssignments.FindAsync(id);
            if (pcpAssignment == null)
            {
                return NotFound();
            }
            return View(pcpAssignment);
        }

        // POST: Pcpa/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("PcpAssignmentId,PcpHeaderId,PlanCode,Cin,Npi,TradingPartnerCode,DataSource")] PcpAssignment pcpAssignment)
        {
            if (id != pcpAssignment.PcpAssignmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pcpAssignment);
                    await _context.SaveChangesAsync();
                    OperationLog logOperation = new OperationLog
                    {
                        Message = "Edit " + pcpAssignment.Cin,
                        ModuleName = "PCPA Edit",
                        OperationTime = DateTime.Now,
                        UserId = User.Identity.Name
                    };
                    _contextLog.OperationLogs.Add(logOperation);
                    await _contextLog.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PcpAssignmentExists(pcpAssignment.PcpAssignmentId))
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
            return View(pcpAssignment);
        }

        // GET: Pcpa/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pcpAssignment = await _context.PcpAssignments
                .FirstOrDefaultAsync(m => m.PcpAssignmentId == id);
            if (pcpAssignment == null)
            {
                return NotFound();
            }

            return View(pcpAssignment);
        }

        // POST: Pcpa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var pcpAssignment = await _context.PcpAssignments.FindAsync(id);
            _context.PcpAssignments.Remove(pcpAssignment);
            await _context.SaveChangesAsync();
            OperationLog logOperation = new OperationLog
            {
                Message = "Delete " + pcpAssignment.Cin,
                ModuleName = "PCPA Delete",
                OperationTime = DateTime.Now,
                UserId = User.Identity.Name
            };
            _contextLog.OperationLogs.Add(logOperation);
            await _contextLog.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PcpAssignmentExists(long id)
        {
            return _context.PcpAssignments.Any(e => e.PcpAssignmentId == id);
        }
    }
}
