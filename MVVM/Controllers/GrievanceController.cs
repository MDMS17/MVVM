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
    public class GrievanceController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public GrievanceController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }
        public async Task<IActionResult> Index()
        {
            GrievanceViewModel model = new GrievanceViewModel();
            model.GrievanceCurrent = await _context.Grievances.Take(int.Parse(model.PageSizeCurrent)).ToListAsync();
            model.PageCurrent = "1";
            model.PageCurrentTotal = Math.Ceiling((decimal)_context.Grievances.Count() / int.Parse(model.PageSizeCurrent)).ToString();
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
                model.GrievanceHistory = await GetGrievanceRecordsHistory(0, historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetGrievanceHistoryPageTotal(historyHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", pageSizeHistory);
                model.tbPageHistory = "1";
                model.historyFirstDisabled = true;
                model.historyPreviousDisabled = true;
                model.historyNextDisabled = false;
                model.historyLastDisabled = false;
            }
            else
            {
                model.GrievanceHistory = new List<McpdGrievance>();
            }
            var errorHeader = await _contextError.McpdHeaders.OrderByDescending(x => x.McpdHeaderId).FirstOrDefaultAsync();
            if (errorHeader != null)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.selectedYearError = errorHeader.ReportingPeriod.Substring(0, 4);
                model.selectedMonthError = errorHeader.ReportingPeriod.Substring(4, 2);
                model.GrievanceError = await GetGrievanceRecordsError(0, errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetGrievanceErrorPageTotal(errorHeader.McpdHeaderId, "All", "All", "", "", "", "", "", "", "", "", "", pageSizeError);
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
                model.GrievanceError = new List<McpdGrievance>();
            }
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int? id, GrievanceViewModel model)
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
                    model.GrievanceCurrent = await GetGrievanceRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceCurrent = await GetGrievanceRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceCurrent = await GetGrievanceRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GRievanceCurrent", model.GrievanceCurrent);
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceCurrent = await GetGrievanceRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceCurrent = await GetGrievanceRecords(pageCurrent, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceHistory = await GetGrievanceRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceHistory = await GetGrievanceRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceHistory = await GetGrievanceRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceHistory = await GetGrievanceRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceHistory = await GetGrievanceRecordsHistory(pageHistory, historyHeaderId, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
                    if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
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
                    model.GrievanceError = await GetGrievanceRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
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
                    model.GrievanceError = await GetGrievanceRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
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
                    model.GrievanceError = await GetGrievanceRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
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
                    model.GrievanceError = await GetGrievanceRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
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
                    model.GrievanceError = await GetGrievanceRecordsError(pageError, errorHeaderId, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                    HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
                    model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
                    if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
                    model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
                    if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
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

        private async Task<List<McpdGrievance>> GetGrievanceRecords(int pageCurrent, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            var result = _context.Grievances.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByRecordType(RecordType).FilterByBenefitType(BenefitType).FilterByDataSource(DataSource)
                .FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId).FilterByReceiveDate(ReceiveDate)
                .FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Skip(pageCurrent * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetGrievancePageTotal(string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            string result = Math.Ceiling((decimal)_context.Grievances.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode)
                .FilterByCin(Cin).FilterByRecordType(RecordType).FilterByBenefitType(BenefitType)
                .FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdGrievance>> GetGrievanceRecordsHistory(int pageNumber, long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            var result = _contextHistory.Grievances.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType).FilterByBenefitType(BenefitType)
                .FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetGrievanceHistoryPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextHistory.Grievances.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType)
                .FilterByBenefitType(BenefitType).FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdGrievance>> GetGrievanceRecordsError(int pageNumber, long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            var result = _contextError.Grievances.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType).FilterByBenefitType(BenefitType)
                .FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Skip(pageNumber * PageSize).Take(PageSize);
            return await Task.FromResult(result.ToList());
        }
        private async Task<string> GetGrievanceErrorPageTotal(long headerId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId, int PageSize)
        {
            string result = Math.Ceiling((decimal)_contextError.Grievances.Where(x => x.McpdHeaderId == headerId)
                .FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType)
                .FilterByBenefitType(BenefitType).FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator)
                .Count() / PageSize).ToString();
            return await Task.FromResult(result);
        }
        private async Task<List<McpdGrievance>> GetGrievanceForDownload(string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId)
        {
            var result = _context.Grievances.FilterByTradingPartner(TradingPartnerCode).FilterByPlanCode(PlanCode).FilterByCin(Cin)
                .FilterByRecordType(RecordType).FilterByBenefitType(BenefitType).FilterByDataSource(DataSource)
                .FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId).FilterByReceiveDate(ReceiveDate)
                .FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdGrievance>> GetGrievanceHistoryForDownload(long historyHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId)
        {
            var result = _contextHistory.Grievances.Where(x => x.McpdHeaderId == historyHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType).FilterByBenefitType(BenefitType)
                .FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator);
            return await Task.FromResult(result.ToList());
        }
        private async Task<List<McpdGrievance>> GetGrievanceErrorForDownload(long errorHeaderId, string TradingPartnerCode, string PlanCode, string Cin, string DataSource, string RecordType, string GrievanceType, string BenefitType, string ExemptIndicator, string ReceiveDate, string GrievanceId, string ParentId)
        {
            var result = _contextError.Grievances.Where(x => x.McpdHeaderId == errorHeaderId).FilterByTradingPartner(TradingPartnerCode)
                .FilterByPlanCode(PlanCode).FilterByCin(Cin).FilterByRecordType(RecordType).FilterByBenefitType(BenefitType)
                .FilterByDataSource(DataSource).FilterByGrievanceId(GrievanceId).FilterByParentId(ParentId)
                .FilterByReceiveDate(ReceiveDate).FilterByGrievanceType(GrievanceType).FilterByExemptIndicator(ExemptIndicator);
            return await Task.FromResult(result.ToList());
        }
        private async Task<GrievanceViewModel> PageSizeChangeCurrent(GrievanceViewModel model)
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
                        case "RecordType":
                            model.RecordTypeCurrent = item.Item2;
                            break;
                        case "GrievanceType":
                            model.GrievanceTypeCurrent = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeCurrent = item.Item2;
                            break;
                        case "ExemptIndicator":
                            model.ExemptIndicatorCurrent = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceCurrent = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateCurrent = item.Item2;
                            break;
                        case "GrievanceId":
                            model.GrievanceIdCurrent = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentIdCurrent = item.Item2;
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
                }
                ii++;
            }
            int pageSizeCurrent = int.Parse(model.PageSizeCurrent);
            model.GrievanceCurrent = await GetGrievanceRecords(0, model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
            model.PageCurrent = "1";
            model.PageCurrentTotal = await GetGrievancePageTotal(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent, pageSizeCurrent);
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
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceCurrent", model.GrievanceCurrent);
            model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
            if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
            model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
            if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();

            GlobalViewModel.PageSizeCurrent = model.PageSizeCurrent;
            return model;
        }
        private async Task<GrievanceViewModel> PageSizeChangeHistory(GrievanceViewModel model)
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
                        case "RecordType":
                            model.RecordTypeHistory = item.Item2;
                            break;
                        case "GrievanceType":
                            model.GrievanceTypeHistory = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeHistory = item.Item2;
                            break;
                        case "ExemptIndicator":
                            model.ExemptIndicatorHistory = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceHistory = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateHistory = item.Item2;
                            break;
                        case "GrievanceId":
                            model.GrievanceIdHistory = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentIdHistory = item.Item2;
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
                }
                ii++;
            }
            long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
            if (historyHeaderId.HasValue)
            {
                int pageSizeHistory = int.Parse(model.PageSizeHistory);
                model.GrievanceHistory = await GetGrievanceRecordsHistory(0, historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
                model.PageHistory = "1";
                model.PageHistoryTotal = await GetGrievanceHistoryPageTotal(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory, pageSizeHistory);
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
                model.GrievanceHistory = new List<McpdGrievance>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "active";
            model.TabActiveError = "";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:purple;";
            model.TabStyleColorError = "color:black;";
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceHistory", model.GrievanceHistory);
            model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
            if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
            model.GrievanceError = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceError");
            if (model.GrievanceError == null) model.GrievanceError = new List<McpdGrievance>();
            GlobalViewModel.PageSizeHistory = model.PageSizeHistory;
            return model;
        }
        private async Task<GrievanceViewModel> PageSizeChangeError(GrievanceViewModel model)
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
                        case "RecordType":
                            model.RecordTypeError = item.Item2;
                            break;
                        case "GrievanceType":
                            model.GrievanceTypeError = item.Item2;
                            break;
                        case "BenefitType":
                            model.BenefitTypeError = item.Item2;
                            break;
                        case "ExemptIndicator":
                            model.ExemptIndicatorError = item.Item2;
                            break;
                        case "DataSource":
                            model.DataSourceError = item.Item2;
                            break;
                        case "ReceiveDate":
                            model.ReceiveDateError = item.Item2;
                            break;
                        case "GrievanceId":
                            model.GrievanceIdError = item.Item2;
                            break;
                        case "ParentId":
                            model.ParentIdError = item.Item2;
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
                }
                ii++;
            }

            long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
            if (errorHeaderId.HasValue)
            {
                int pageSizeError = int.Parse(model.PageSizeError);
                model.GrievanceError = await GetGrievanceRecordsError(0, errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
                model.PageError = "1";
                model.PageErrorTotal = await GetGrievanceErrorPageTotal(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError, pageSizeError);
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
                model.GrievanceError = new List<McpdGrievance>();
            }
            model.TabActiveCurrent = "";
            model.TabActiveHistory = "";
            model.TabActiveError = "active";
            model.TabStyleColorCurrent = "color:black;";
            model.TabStyleColorHistory = "color:black;";
            model.TabStyleColorError = "color:purple;";
            HttpContext.Session.Set<List<McpdGrievance>>("GrievanceError", model.GrievanceError);
            model.GrievanceCurrent = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceCurrent");
            if (model.GrievanceCurrent == null) model.GrievanceCurrent = new List<McpdGrievance>();
            model.GrievanceHistory = HttpContext.Session.Get<List<McpdGrievance>>("GrievanceHistory");
            if (model.GrievanceHistory == null) model.GrievanceHistory = new List<McpdGrievance>();
            GlobalViewModel.PageSizeError = model.PageSizeError;
            return model;
        }
        public async Task<IActionResult> DownloadFile(long? id, GrievanceViewModel model)
        {
            List<McpdGrievance> grievances = new List<McpdGrievance>();
            string GrievanceType = "";
            string exportType = "";
            if (id == 4)
            {
                //download current
                grievances = await GetGrievanceForDownload(model.selectedTradingPartner, model.PlanCodeCurrent, model.CinCurrent, model.DataSourceCurrent, model.RecordTypeCurrent, model.GrievanceTypeCurrent, model.BenefitTypeCurrent, model.ExemptIndicatorCurrent, model.ReceiveDateCurrent, model.GrievanceIdCurrent, model.ParentIdCurrent);
                GrievanceType = "grievance_staging";
                exportType = model.selectedExport;
            }
            else if (id == 5)
            {
                //download history
                long? historyHeaderId = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearHistory + model.selectedMonthHistory)?.McpdHeaderId;
                if (historyHeaderId.HasValue)
                {
                    grievances = await GetGrievanceHistoryForDownload(historyHeaderId.Value, model.selectedTradingPartnerHistory, model.PlanCodeHistory, model.CinHistory, model.DataSourceHistory, model.RecordTypeHistory, model.GrievanceTypeHistory, model.BenefitTypeHistory, model.ExemptIndicatorHistory, model.ReceiveDateHistory, model.GrievanceIdHistory, model.ParentIdHistory);
                }
                GrievanceType = "grievance_history";
                exportType = model.selectedExportHistory;
            }
            else if (id == 6)
            {
                //download error
                long? errorHeaderId = _contextError.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == model.selectedYearError + model.selectedMonthError)?.McpdHeaderId;
                if (errorHeaderId.HasValue)
                {
                    grievances = await GetGrievanceErrorForDownload(errorHeaderId.Value, model.selectedTradingPartnerError, model.PlanCodeError, model.CinError, model.DataSourceError, model.RecordTypeError, model.GrievanceTypeError, model.BenefitTypeError, model.ExemptIndicatorError, model.ReceiveDateError, model.GrievanceIdError, model.ParentIdError);
                }
                GrievanceType = "grievance_error";
                exportType = model.selectedExportError;
            }
            if (exportType == ".csv")
            {
                var columnHeader = new string[] { "McpdHeaderId", "PlanCode", "Cin", "GrievanceId", "RecordType", "ParentGrievanceId", "GrievanceReceivedDate", "GrievanceType", "BenefitType", "ExemptIndicator", "TradingPartnerCode", "ErrorMessage", "DataSource" };
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(string.Join(",", columnHeader));
                grievances.ForEach(x => sb.AppendLine($"{x.McpdHeaderId.ToString()},{x.PlanCode },{x.Cin },{x.GrievanceId},{x.RecordType},{x.ParentGrievanceId},{x.GrievanceReceivedDate},{x.GrievanceType},{x.BenefitType},{x.ExemptIndicator},{x.TradingPartnerCode},{x.ErrorMessage},{x.DataSource}"));
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/csv", GrievanceType + DateTime.Today.ToString("yyyyMMdd") + ".csv");
            }
            else if(exportType=="json")
            {
                McpdHeader mcpdHeader = _context.McpdHeaders.Find(grievances[0].McpdHeaderId);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("{\"header\":");
                sb.Append(JsonOperations.GetMcpdHeaderJson(mcpdHeader));
                sb.Append($",\"{GrievanceType}\":");
                sb.Append(JsonOperations.GetGrievanceJson(grievances));
                sb.Append("}");
                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                return File(buffer, "text/json", GrievanceType + DateTime.Today.ToString("yyyyMMdd") + ".json");
            }
            else
            {
                string fileName = GrievanceType + DateTime.Today.ToString("yyyyMMdd") + ".xlsx";
                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(grievances.ToDataTable());
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
        }
        // GET: Grievance/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grievance = await _context.Grievances
                .FirstOrDefaultAsync(m => m.McpdGrievanceId == id);
            if (grievance == null)
            {
                return NotFound();
            }

            return View(grievance);
        }

        // GET: Grievance/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Grievance/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("McpdGrievanceId,McpdHeaderId,PlanCode,Cin,GrievanceId,RecordType,ParentGrievanceId,GrievanceReceivedDate,GrievanceType,BenefitType,ExemptIndicator,TradingPartnerCode,ErrorMessage,DataSource")] McpdGrievance grievance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(grievance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(grievance);
        }

        // GET: Grievance/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grievance = await _context.Grievances.FindAsync(id);
            if (grievance == null)
            {
                return NotFound();
            }
            return View(grievance);
        }

        // POST: Grievance/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("McpdGrievanceId,McpdHeaderId,PlanCode,Cin,GrievanceId,RecordType,ParentGrievanceId,GrievanceReceivedDate,GrievanceType,BenefitType,ExemptIndicator,TradingPartnerCode,ErrorMessage,DataSource")] McpdGrievance grievance)
        {
            if (id != grievance.McpdGrievanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(grievance);
                    await _context.SaveChangesAsync();
                    OperationLog logOperation = new OperationLog
                    {
                        Message = "Edit " + grievance.GrievanceId,
                        ModuleName = "Grievance Edit",
                        OperationTime = DateTime.Now,
                        UserId = User.Identity.Name
                    };
                    _contextLog.OperationLogs.Add(logOperation);
                    await _contextLog.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GrievanceExists(grievance.McpdGrievanceId))
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
            return View(grievance);
        }

        // GET: Grievance/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grievance = await _context.Grievances
                .FirstOrDefaultAsync(m => m.McpdGrievanceId == id);
            if (grievance == null)
            {
                return NotFound();
            }

            return View(grievance);
        }

        // POST: Grievance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var grievance = await _context.Grievances.FindAsync(id);
            _context.Grievances.Remove(grievance);
            await _context.SaveChangesAsync();
            OperationLog logOperation = new OperationLog
            {
                Message = "Delete " + grievance.GrievanceId,
                ModuleName = "Grievance Delete",
                OperationTime = DateTime.Now,
                UserId = User.Identity.Name
            };
            _contextLog.OperationLogs.Add(logOperation);
            await _contextLog.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GrievanceExists(long id)
        {
            return _context.Grievances.Any(e => e.McpdGrievanceId == id);
        }
    }
}


