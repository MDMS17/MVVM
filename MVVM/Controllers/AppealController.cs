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
using MCPDIP.Web.Extensions;

namespace mcpdandpcpa.Controllers
{
    public class AppealController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public AppealController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }
        public async Task<IActionResult> Index()
        {
            AppealViewModel model = new AppealViewModel();
            model.AppealCurrent = await _context.Appeals.Take(int.Parse(model.PageSizeCurrent)).ToListAsync();
            model.PageCurrent = "1";
            model.PageCurrentTotal = Math.Ceiling((decimal)_context.Appeals.Count() / int.Parse(model.PageSizeCurrent)).ToString();
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
                model.AppealHistory = await GetAppealRecordsHistory(0, historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetAppealHistoryPageTotal(historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.tbPageHistory = "1";
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                model.historyNextDisabled = false;
                model.historyLastDisabled = false;
            }
            else
            {
                model.AppealHistory = new List<McpdAppeal>();
            }
            var errorHeader = await _contextError.McpdHeaders.OrderByDescending(x => x.McpdHeaderId).FirstOrDefaultAsync();
            if (errorHeader != null)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.selectedYearError = errorHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthError = errorHeader.ReportingPeriod.Substring(4, 2);
                model.AppealError = await GetAppealRecordsError(0, errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetAppealErrorPageTotal(errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", "", "", "", "", "", pageSizeError);
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
                model.AppealError = new List<McpdAppeal>();
            }
            HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
            HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
            HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? id, AppealViewModel model)
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
                    model.AppealCurrent = await GetAppealRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealCurrent = await GetAppealRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealCurrent = await GetAppealRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealCurrent = await GetAppealRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealCurrent = await GetAppealRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealHistory = await GetAppealRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealHistory = await GetAppealRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealHistory = await GetAppealRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealHistory = await GetAppealRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealHistory = await GetAppealRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
                    if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
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
                    model.AppealError = await GetAppealRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
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
                    model.AppealError = await GetAppealRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
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
                    model.AppealError = await GetAppealRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
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
                    model.AppealError = await GetAppealRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
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
                    model.AppealError = await GetAppealRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
                    model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
                    if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
                    model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
                    if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
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

        private async Task<List<McpdAppeal>> GetAppealRecords(int pageCurrent, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            var result = _context.Appeals.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Skip(pageCurrent * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetAppealPageTotal(string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_context.Appeals.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdAppeal>> GetAppealRecordsHistory(int pageNumber, long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            var result = _contextHistory.Appeals.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetAppealHistoryPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextHistory.Appeals.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdAppeal>> GetAppealRecordsError(int pageNumber, long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            var result = _contextError.Appeals.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetAppealErrorPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextError.Appeals.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdAppeal>> GetAppealForDownload(string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource)
        {
            var result = _context.Appeals.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdAppeal>> GetAppealHistoryForDownload(long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource)
        {
            var result = _contextHistory.Appeals.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdAppeal>> GetAppealErrorForDownload(long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string AppealId, string RecordType, string ParentGrievanceId, string ParentAppealId, string ReceiveDate, string ActionDate, string AppealType, string BenefitType, string StatusIndicator, string ResolutionDate, string OverturnIndicator, string ExpediteIndicator, string DataSource)
        {
            var result = _contextError.Appeals.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByAppealId(AppealId).FilterByRecordType(RecordType).FilterByParentGrievanceId(ParentGrievanceId)
                .FilterByParentAppealId(ParentAppealId).FilterByAppealReceiveDate(ReceiveDate).FilterByActionDate(ActionDate)
                .FilterByAppealType(AppealType).FilterByBenefitType(BenefitType).FilterByStatusIndicator(StatusIndicator)
                .FilterByResolutionDate(ResolutionDate).FilterByOverturnIndicator(OverturnIndicator).FilterByExpediteIndicator(ExpediteIndicator)
                .FilterByDataSource(DataSource);
            return await Task.FromResult(result.ToList());
        }
        private async Task<AppealViewModel> PageSizeChangeCurrent(AppealViewModel model)
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
                        case "AppealId":
                            model.AppealIdCurrent = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeCurrent = item.Item2;
                            break;
                        case "ParentGrievanceId":
                            model.ParentGrievanceIdCurrent = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentAppealIdCurrent = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateCurrent = item.Item2;
                            break;
                        case "ActionDate":
                            model.ActionDateCurrent = item.Item2;
                            break;
                        case "AppealType":
                            model.AppealTypeCurrent = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeCurrent = item.Item2;
                            break;
                        case "StatusIndicator":
                            model.StatusIndicatorCurrent = item.Item2;
                            break;
                        case "ResolutionDate":
                            model.ResolutionDateCurrent = item.Item2;
                            break;
                        case "OverturnIndicator":
                            model.OverturnIndicatorCurrent = item.Item2;
                            break;
                        case "ExpediteIndicator":
                            model.ExpediteIndicatorCurrent = item.Item2;
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
                }
                ii++;
            }
            int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
            model.AppealCurrent = await GetAppealRecords(0, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
            model.PageCurrent = "1";
            model.PageCurrentTotal = await GetAppealPageTotal(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent, pageSizeCurrent);
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
            HttpContext.Session.Set<List<McpdAppeal>>("AppealCurrent", model.AppealCurrent);
            model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
            if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
            model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
            if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();

            GlobalViewModel.PageSizeCurrent = model.PageSizeCurrent;
            return model;
        }
        private async Task<AppealViewModel> PageSizeChangeHistory(AppealViewModel model)
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
                        case "AppealId":
                            model.AppealIdHistory = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeHistory = item.Item2;
                            break;
                        case "ParentGrievanceId":
                            model.ParentGrievanceIdHistory = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentAppealIdHistory = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateHistory = item.Item2;
                            break;
                        case "ActionDate":
                            model.ActionDateHistory = item.Item2;
                            break;
                        case "AppealType":
                            model.AppealTypeHistory = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeHistory = item.Item2;
                            break;
                        case "StatusIndicator":
                            model.StatusIndicatorHistory = item.Item2;
                            break;
                        case "ResolutionDate":
                            model.ResolutionDateHistory = item.Item2;
                            break;
                        case "OverturnIndicator":
                            model.OverturnIndicatorHistory = item.Item2;
                            break;
                        case "ExpediteIndicator":
                            model.ExpediteIndicatorHistory = item.Item2;
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
                }
                ii++;
            }

            long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
            if (historyHeaderId.HasValue)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.AppealHistory = await GetAppealRecordsHistory(0, historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetAppealHistoryPageTotal(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory, pageSizeHistory);
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
                model.AppealHistory = new List<McpdAppeal>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "active";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:purple;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<McpdAppeal>>("AppealHistory", model.AppealHistory);
            model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
            if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
            model.AppealError = HttpContext.Session.Get<List<McpdAppeal>>("AppealError");
            if (model.AppealError == null) model.AppealError = new List<McpdAppeal>();
            GlobalViewModel.PageSizeHistory = model.PageSizeHistory;
            return model;
        }
        private async Task<AppealViewModel> PageSizeChangeError(AppealViewModel model)
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
                        case "AppealId":
                            model.AppealIdError = item.Item2;
                            break;
                        case "RecordType":
                            model.RecordTypeError = item.Item2;
                            break;
                        case "ParentGrievanceId":
                            model.ParentGrievanceIdError = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentAppealIdError = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateError = item.Item2;
                            break;
                        case "ActionDate":
                            model.ActionDateError = item.Item2;
                            break;
                        case "AppealType":
                            model.AppealTypeError = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeError = item.Item2;
                            break;
                        case "StatusIndicator":
                            model.StatusIndicatorError = item.Item2;
                            break;
                        case "ResolutionDate":
                            model.ResolutionDateError = item.Item2;
                            break;
                        case "OverturnIndicator":
                            model.OverturnIndicatorError = item.Item2;
                            break;
                        case "ExpediteIndicator":
                            model.ExpediteIndicatorError = item.Item2;
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
                }
                ii++;
            }

            long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
            if (errorHeaderId.HasValue)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.AppealError = await GetAppealRecordsError(0, errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetAppealErrorPageTotal(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError, pageSizeError);
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
                model.AppealError = new List<McpdAppeal>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "";
            model.TabActiveError = "active";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:purple;";
            HttpContext.Session.Set<List<McpdAppeal>>("AppealError", model.AppealError);
            model.AppealCurrent = HttpContext.Session.Get<List<McpdAppeal>>("AppealCurrent");
            if (model.AppealCurrent == null) model.AppealCurrent = new List<McpdAppeal>();
            model.AppealHistory = HttpContext.Session.Get<List<McpdAppeal>>("AppealHistory");
            if (model.AppealHistory == null) model.AppealHistory = new List<McpdAppeal>();
            GlobalViewModel.PageSizeError = model.PageSizeError;
            return model;
        }
        public async Task<IActionResult> DownloadFile(long? id, AppealViewModel model)
        {
            List<McpdAppeal> Appeals = new List<McpdAppeal>();
            string AppealType = "";
            string exportType = "";
            if (id == 4)
            {
                //download current
                Appeals = await GetAppealForDownload(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.AppealIdCurrent, model.RecordTypeCurrent, model.ParentGrievanceIdCurrent, model.ParentAppealIdCurrent, model.ReceiveDateCurrent, model.ActionDateCurrent, model.AppealTypeCurrent, model.BenefitTypeCurrent, model.StatusIndicatorCurrent, model.ResolutionDateCurrent, model.OverturnIndicatorCurrent, model.ExpediteIndicatorCurrent, model.DataSourceCurrent);
                AppealType = "Appeal_staging";
                exportType = model.selectedExport;
            }
            else if (id == 5)
            {
                //download history
                long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
                if (historyHeaderId.HasValue)
                {
                    Appeals = await GetAppealHistoryForDownload(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.AppealIdHistory, model.RecordTypeHistory, model.ParentGrievanceIdHistory, model.ParentAppealIdHistory, model.ReceiveDateHistory, model.ActionDateHistory, model.AppealTypeHistory, model.BenefitTypeHistory, model.StatusIndicatorHistory, model.ResolutionDateHistory, model.OverturnIndicatorHistory, model.ExpediteIndicatorHistory, model.DataSourceHistory);
                }
                AppealType = "Appeal_history";
                exportType = model.selectedExportHistory;
            }
            else if (id == 6)
            {
                //download error
                long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
                if (errorHeaderId.HasValue)
                {
                    Appeals = await GetAppealErrorForDownload(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.AppealIdError, model.RecordTypeError, model.ParentGrievanceIdError, model.ParentAppealIdError, model.ReceiveDateError, model.ActionDateError, model.AppealTypeError, model.BenefitTypeError, model.StatusIndicatorError, model.ResolutionDateError, model.OverturnIndicatorError, model.ExpediteIndicatorError, model.DataSourceError);
                }
                AppealType = "Appeal_error";
                exportType = model.selectedExportError;
            }
            if (exportType == ".csv")
            {
                var columnHeader = new string[] { "McpdHeaderId", "PlanCode", "Cin", "AppealId", "RecordType", "ParentGrievanceId", "ParentAppealId", "AppealReceivedDate", "AppealType", "BenefitType", "ApealResolutionStatusIndicator", "AppealResolutionDate", "PartiallyOverturnIndicator", "ExpediteIndicator", "TradingPartnerCode", "ErrorMessage", "DataSource" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(",", columnHeader));
                Appeals.ForEach(x => sb.AppendLine($"{x.McpdHeaderId.ToString()},{x.PlanCode },{x.Cin },{x.AppealId},{x.RecordType},{x.ParentGrievanceId},{x.ParentAppealId},{x.AppealReceivedDate},{x.AppealType},{x.BenefitType},{x.AppealResolutionStatusIndicator},{x.AppealResolutionDate},{x.PartiallyOverturnIndicator},{x.ExpeditedIndicator},{x.TradingPartnerCode},{x.ErrorMessage},{x.DataSource}"));
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/csv", AppealType + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            }
            else if (exportType == "json")
            {
                McpdHeader mcpdHeader = _context.McpdHeaders.Find(Appeals[0].McpdHeaderId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{\"header\":");
                sb.Append(JsonOperations.GetMcpdHeaderJson(mcpdHeader));
                sb.Append($",\"{AppealType}\":");
                sb.Append(JsonOperations.GetAppealJson(Appeals));
                sb.Append("}");
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/json", AppealType + DateTime.Today.ToString("yyyyMMdd") + ".json");
            }
            else
            {
                string fileName = AppealType + DateTime.Today.ToString("yyyyMMdd") + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(Appeals.ToDataTable());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
        }
        // GET: Appeal/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdAppeal = await _context.Appeals
                .FirstOrDefaultAsync(m => m.McpdAppealId == id);
            if (mcpdAppeal == null)
            {
                return NotFound();
            }

            return View(mcpdAppeal);
        }

        // GET: Appeal/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Appeal/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("McpdAppealId,McpdHeaderId,PlanCode,Cin,AppealId,RecordType,ParentGrievanceId,ParentAppealId,AppealReceivedDate,NoticeOfActionDate,AppealType,BenefitType,AppealResolutionStatusIndicator,AppealResolutionDate,PartiallyOverturnIndicator,ExpeditedIndicator,TradingPartnerCode,ErrorMessage,DataSource")] McpdAppeal mcpdAppeal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mcpdAppeal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdAppeal);
        }

        // GET: Appeal/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdAppeal = await _context.Appeals.FindAsync(id);
            if (mcpdAppeal == null)
            {
                return NotFound();
            }
            return View(mcpdAppeal);
        }

        // POST: Appeal/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("McpdAppealId,McpdHeaderId,PlanCode,Cin,AppealId,RecordType,ParentGrievanceId,ParentAppealId,AppealReceivedDate,NoticeOfActionDate,AppealType,BenefitType,AppealResolutionStatusIndicator,AppealResolutionDate,PartiallyOverturnIndicator,ExpeditedIndicator,TradingPartnerCode,DataSource")] McpdAppeal mcpdAppeal)
        {
            if (id != mcpdAppeal.McpdAppealId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mcpdAppeal);
                    await _context.SaveChangesAsync();
                    OperationLog logOperation = new OperationLog
                    {
                        Message = "Edit " + mcpdAppeal.AppealId,
                        ModuleName = "Appeal Edit",
                        OperationTime = DateTime.Now,
                        UserId = User.Identity.Name
                    };
                    _contextLog.OperationLogs.Add(logOperation);
                    await _contextLog.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!McpdAppealExists(mcpdAppeal.McpdAppealId))
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
            return View(mcpdAppeal);
        }

        // GET: Appeal/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdAppeal = await _context.Appeals
                .FirstOrDefaultAsync(m => m.McpdAppealId == id);
            if (mcpdAppeal == null)
            {
                return NotFound();
            }

            return View(mcpdAppeal);
        }

        // POST: Appeal/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var mcpdAppeal = await _context.Appeals.FindAsync(id);
            _context.Appeals.Remove(mcpdAppeal);
            await _context.SaveChangesAsync();
            OperationLog logOperation = new OperationLog
            {
                Message = "Delete " + mcpdAppeal.AppealId,
                ModuleName = "Appeal Delete",
                OperationTime = DateTime.Now,
                UserId = User.Identity.Name
            };
            _contextLog.OperationLogs.Add(logOperation);
            return RedirectToAction(nameof(Index));
        }

        private bool McpdAppealExists(long id)
        {
            return _context.Appeals.Any(e => e.McpdAppealId == id);
        }
    }
}
