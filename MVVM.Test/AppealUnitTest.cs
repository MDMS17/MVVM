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
    public class AppealUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private AppealController _appealController;

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
            List<McpdAppeal> appeals = new List<McpdAppeal>()
            {
                new McpdAppeal()
                {
                     McpdAppealId=1,
                     McpdHeaderId=1,
                     PlanCode="305",
                     Cin="32001378A",
                     AppealId="305A200627191721",
                     RecordType="Original",
                     AppealReceivedDate="20200302",
                     NoticeOfActionDate="20200302",
                     AppealType="Denied",
                     BenefitType="Not Benefit Related",
                     AppealResolutionStatusIndicator="Resolved in Favor of Plan",
                     AppealResolutionDate="20200302",
                     PartiallyOverturnIndicator="Not a Partially Overturned Appeal",
                     ExpeditedIndicator="Expedited",
                     TradingPartnerCode="IEHP"
                },
                new McpdAppeal()
                {
                     McpdAppealId=2,
                     McpdHeaderId=1,
                     PlanCode="305",
                     Cin="32001379A",
                     AppealId="305A200633680331",
                     RecordType="Original",
                     AppealReceivedDate="20200303",
                     NoticeOfActionDate="20200304",
                     AppealType="Denied",
                     BenefitType="Emergency",
                     AppealResolutionStatusIndicator="Resolved in Favor of Member",
                     AppealResolutionDate="20200304",
                     PartiallyOverturnIndicator="Partially Overturned Appeal",
                     ExpeditedIndicator="Expedited",
                     TradingPartnerCode="IEHP"
                }
            };
            _context.Appeals.AddRange(appeals);
            _appealController = new AppealController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestAppealController()
        {
            AppealViewModel model = new AppealViewModel();
            model.AppealCurrent = _context.Appeals.Local.Select(x => new McpdAppeal
            {
                Cin = x.Cin,
                BenefitType = x.BenefitType,
                DataSource = x.DataSource,
                ErrorMessage = x.ErrorMessage,
                AppealId = x.AppealId,
                AppealReceivedDate = x.AppealReceivedDate,
                AppealResolutionDate = x.AppealResolutionDate,
                AppealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                AppealType = x.AppealType,
                ExpeditedIndicator = x.ExpeditedIndicator,
                McpdAppealId = x.McpdAppealId,
                NoticeOfActionDate = x.NoticeOfActionDate,
                ParentAppealId = x.ParentAppealId,
                PartiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                McpdHeaderId = x.McpdHeaderId,
                ParentGrievanceId = x.ParentGrievanceId,
                PlanCode = x.PlanCode,
                RecordType = x.RecordType,
                TradingPartnerCode = x.TradingPartnerCode
            }).ToList();
            model.AppealHistory = new List<McpdAppeal>();
            model.AppealError = new List<McpdAppeal>();
            var result = _appealController.Index(991, model);
            Assert.IsTrue(result.IsCompleted);
        }
    }
}
