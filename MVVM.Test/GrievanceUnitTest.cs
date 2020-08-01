using Microsoft.VisualStudio.TestTools.UnitTesting;
using mcpdandpcpa.Controllers;
using mcpdandpcpa.Models;
using mcpdipData;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;

namespace MCPDIP.Test
{
    [TestClass]
    public class GrievanceUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private GrievanceController _grievanceController;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<StagingContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var optionh = new DbContextOptionsBuilder<HistoryContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var optione = new DbContextOptionsBuilder<ErrorContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var optionl = new DbContextOptionsBuilder<LogContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _context = new StagingContext(options);
            _contextHistory = new HistoryContext(optionh);
            _contextError = new ErrorContext(optione);
            _contextLog = new LogContext(optionl);
            List<McpdGrievance> grievances = new List<McpdGrievance>()
            {
                new McpdGrievance()
                {
                     McpdGrievanceId=2,
                     McpdHeaderId=1,
                     PlanCode="305",
                     Cin="32001378A",
                     GrievanceId="305G200646311091",
                     RecordType="Original",
                     GrievanceReceivedDate="20200303",
                     GrievanceType="Case Management / Care Coordination",
                     BenefitType="Palliative Care",
                     ExemptIndicator="Not Exempt",
                     TradingPartnerCode="IEHP"
                },
                new McpdGrievance()
                {
                    McpdGrievanceId=3,
                    McpdHeaderId=1,
                    PlanCode="305",
                    Cin="32001379A",
                    GrievanceId="305G200633523111",
                    RecordType="Original",
                    GrievanceReceivedDate="20200302",
                    GrievanceType="Physical Access",
                    BenefitType="Outpatient Physical Health",
                     ExemptIndicator="Not Exempt",
                     TradingPartnerCode="IEHP"
                }
            };
            _context.Grievances.AddRange(grievances);
            _grievanceController = new GrievanceController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestGrievanceController()
        {
            GrievanceViewModel model = new GrievanceViewModel();
            model.GrievanceCurrent = _context.Grievances.Local.Select(x => new McpdGrievance
            {
                Cin = x.Cin,
                BenefitType = x.BenefitType,
                DataSource = x.DataSource,
                ErrorMessage = x.ErrorMessage,
                ExemptIndicator = x.ExemptIndicator,
                GrievanceId = x.GrievanceId,
                GrievanceReceivedDate = x.GrievanceReceivedDate,
                GrievanceType = x.GrievanceType,
                McpdGrievanceId = x.McpdGrievanceId,
                McpdHeaderId = x.McpdHeaderId,
                ParentGrievanceId = x.ParentGrievanceId,
                PlanCode = x.PlanCode,
                RecordType = x.RecordType,
                TradingPartnerCode = x.TradingPartnerCode
            }).ToList();
            model.GrievanceHistory = new List<McpdGrievance>();
            model.GrievanceError = new List<McpdGrievance>();
            var result = _grievanceController.Index(991, model);
            Assert.IsTrue(result.IsCompleted);
        }
    }
}
