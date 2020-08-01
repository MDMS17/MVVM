using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;
using mcpdandpcpa.Extensions;

namespace mcpdandpcpa.Controllers
{
    public class HomeController : Controller
    {
        private readonly LogContext _context;

        public HomeController(LogContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            DashboardViewModel model = new DashboardViewModel();
            model.ProcessLogs = await GetProcessLogs();
            return View(model);
        }
        public async Task<IActionResult> SubmissionLog()
        {
            DashboardViewModel model = new DashboardViewModel();
            model.SubmissionLogs = await GetSubmissionLogs();
            return View(model);
        }
        private async Task<List<ProcessLogDisplayModel>> GetProcessLogs()
        {
            var result = _context.ProcessLogs.OrderByDescending(x => x.LogId).Select(x => new ProcessLogDisplayModel
            {
                TradingPartnerCode = x.TradingPartnerCode,
                RecordYear = x.RecordYear,
                RecordMonth = x.RecordMonth,
                GrievanceTotal = x.GrievanceTotal,
                GrievanceSubmits = x.GrievanceSubmits,
                GrievanceErrors = x.GrievanceErrors,
                AppealTotal = x.AppealTotal,
                AppealSubmits = x.AppealSubmits,
                AppealErrors = x.AppealErrors,
                COCTotal = x.COCTotal,
                COCSubmits = x.COCSubmits,
                COCErrors = x.COCErrors,
                OONTotal = x.OONTotal,
                OONSubmits = x.OONSubmits,
                OONErrors = x.OONErrors,
                PCPATotal = x.PCPATotal,
                PCPASubmits = x.PCPASubmits,
                PCPAErrors = x.PCPAErrors,
                RunStatus = x.RunStatus,
                RunTime = x.RunTime,
                RunBy = x.RunBy
            }).PrepareDisplay<ProcessLogDisplayModel>();

            return await Task.FromResult(result.ToList());
        }
        private async Task<List<SubmissionLogDisplayModel>> GetSubmissionLogs()
        {
            var result = _context.SubmissionLogs.OrderByDescending(x => x.SubmissionId).Select(x => new SubmissionLogDisplayModel
            {
                RecordYear = x.RecordYear,
                RecordMonth = x.RecordMonth,
                FileName = x.FileName,
                FileType = x.FileType,
                SubmitterName = x.SubmitterName,
                SubmissionDate = x.SubmissionDate,
                ValidationStatus = x.ValidationStatus,
                TotalGrievanceSubmitted = x.TotalGrievanceSubmitted,
                TotalGrievanceAccepted = x.TotalGrievanceAccepted,
                TotalGrievanceRejected = x.TotalGrievanceRejected,
                TotalAppealSubmitted = x.TotalAppealSubmitted,
                TotalAppealAccepted = x.TotalAppealAccepted,
                TotalAppealRejected = x.TotalAppealRejected,
                TotalCOCSubmitted = x.TotalCOCSubmitted,
                TotalCOCAccepted = x.TotalCOCAccepted,
                TotalCOCRejected = x.TotalCOCRejected,
                TotalOONSubmitted = x.TotalOONSubmitted,
                TotalOONAccepted = x.TotalOONAccepted,
                TotalOONRejected = x.TotalOONRejected,
                TotalPCPASubmitted = x.TotalPCPASubmitted,
                TotalPCPAAccepted = x.TotalPCPAAccepted,
                TotalPCPARejected = x.TotalPCPARejected,
                UpdateDate = x.UpdateDate,
                UpdateBy = x.UpdateBy
            }).PrepareDisplay<SubmissionLogDisplayModel>();
            return await Task.FromResult(result.ToList());
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult Index(int? id, DashboardViewModel model)
        {
            //this is for test only, id will be brought in as 991
            return View(model);
        }
    }
}

