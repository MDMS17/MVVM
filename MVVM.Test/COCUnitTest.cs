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
    public class COCUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private CocController _cocController;

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
            List<McpdContinuityOfCare> cocs = new List<McpdContinuityOfCare>()
            {
                new McpdContinuityOfCare()
                {
                     McpdContinuityOfCareId=1,
                     McpdHeaderId=1,
                     PlanCode="305",
                     Cin="32001378A",
                     CocId="202007100001",
                     RecordType="Original",
                     CocReceivedDate="20200501",
                     CocType="Other",
                     BenefitType="Emergency",
                     CocDispositionIndicator="Approved",
                     CocExpirationDate="20201231",
                     SubmittingProviderNpi="1437335148",
                     CocProviderNpi="1205102068",
                     MerExemptionId="292092",
                     TradingPartnerCode="IEHP"
                },
                new McpdContinuityOfCare()
                {
                     McpdContinuityOfCareId=2,
                     McpdHeaderId=1,
                     PlanCode="306",
                     Cin="32001388A",
                     CocId="202007100002",
                     RecordType="Original",
                     CocReceivedDate="20200501",
                     CocType="Other",
                     BenefitType="Emergency",
                     CocDispositionIndicator="Approved",
                     CocExpirationDate="20201231",
                     SubmittingProviderNpi="1801270137",
                     CocProviderNpi="1720178478",
                     MerExemptionId="293093",
                     TradingPartnerCode="IEHP"
                }
            };
            _context.McpdContinuityOfCare.AddRange(cocs);
            _cocController = new CocController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestCOCController()
        {
            CocViewModel model = new CocViewModel();
            model.CocCurrent = _context.McpdContinuityOfCare.Local.Select(x => new McpdContinuityOfCare
            {
                McpdContinuityOfCareId = x.McpdContinuityOfCareId,
                McpdHeaderId = x.McpdHeaderId,
                PlanCode = x.PlanCode,
                Cin = x.Cin,
                CocId = x.CocId,
                RecordType = x.RecordType,
                ParentCocId = x.ParentCocId,
                CocReceivedDate = x.CocReceivedDate,
                CocType = x.CocType,
                BenefitType = x.BenefitType,
                CocDispositionIndicator = x.CocDispositionIndicator,
                CocExpirationDate = x.CocExpirationDate,
                CocDenialReasonIndicator = x.CocDenialReasonIndicator,
                SubmittingProviderNpi = x.SubmittingProviderNpi,
                CocProviderNpi = x.CocProviderNpi,
                ProviderTaxonomy = x.ProviderTaxonomy,
                MerExemptionId = x.MerExemptionId,
                ExemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                ExemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                MerCocDispositionIndicator = x.MerCocDispositionIndicator,
                MerCocDispositionDate = x.MerCocDispositionDate,
                ReasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator,
                TradingPartnerCode = x.TradingPartnerCode
            }).ToList();
            model.CocHistory = new List<McpdContinuityOfCare>();
            model.CocError = new List<McpdContinuityOfCare>();
            var result = _cocController.Index(991, model);
            Assert.IsTrue(result.IsCompleted);
        }
    }
}
