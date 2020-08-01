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
    public class OONUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private OonController _oonController;

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
            List<McpdOutOfNetwork> oons = new List<McpdOutOfNetwork>()
            {
                new McpdOutOfNetwork()
                {
                     McpdOutOfNetworkId=1,
                     McpdHeaderId=1,
                     PlanCode="305",
                     Cin="32001379A",
                     OonId="202006010001",
                     RecordType="Original",
                     OonRequestReceivedDate="20200601",
                     ReferralRequestReasonIndicator="Other",
                     OonResolutionStatusIndicator="Pending",
                     SpecialistProviderNpi="1386758225",
                     ProviderTaxonomy="207RC0000X",
                     ServiceLocationAddressLine1="100 Test Dr",
                     ServiceLocationCity="Los Angeles",
                     ServiceLocationState="CA",
                     ServiceLocationZip="90101",
                     ServiceLocationCountry="US",
                     TradingPartnerCode="IEHP"
                },
                new McpdOutOfNetwork()
                {
                     McpdOutOfNetworkId=2,
                     McpdHeaderId=1,
                     PlanCode="306",
                     Cin="32001389A",
                     OonId="202006010002",
                     RecordType="Original",
                     OonRequestReceivedDate="20200601",
                     ReferralRequestReasonIndicator="Other",
                     OonResolutionStatusIndicator="Pending",
                     SpecialistProviderNpi="1861662025",
                     ProviderTaxonomy="207RC0000X",
                     ServiceLocationAddressLine1="200 Test Dr",
                     ServiceLocationCity="Los Angeles",
                     ServiceLocationState="CA",
                     ServiceLocationZip="90201",
                     ServiceLocationCountry="US",
                     TradingPartnerCode="IEHP"
                }
            };
            _context.McpdOutOfNetwork.AddRange(oons);
            _oonController = new OonController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestOONController()
        {
            OonViewModel model = new OonViewModel();
            model.OonCurrent = _context.McpdOutOfNetwork.Local.Select(x => new McpdOutOfNetwork
            {
                Cin = x.Cin,
                DataSource = x.DataSource,
                ErrorMessage = x.ErrorMessage,
                McpdHeaderId = x.McpdHeaderId,
                McpdOutOfNetworkId = x.McpdOutOfNetworkId,
                OonId = x.OonId,
                OonRequestReceivedDate = x.OonRequestReceivedDate,
                OonRequestResolvedDate = x.OonRequestResolvedDate,
                OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                ParentOonId = x.ParentOonId,
                PartialApprovalExplanation = x.PartialApprovalExplanation,
                PlanCode = x.PlanCode,
                ProviderTaxonomy = x.ProviderTaxonomy,
                RecordType = x.RecordType,
                ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                ServiceLocationCity = x.ServiceLocationCity,
                ServiceLocationCountry = x.ServiceLocationCountry,
                ServiceLocationState = x.ServiceLocationState,
                ServiceLocationZip = x.ServiceLocationZip,
                SpecialistProviderNpi = x.SpecialistProviderNpi,
                TradingPartnerCode = x.TradingPartnerCode
            }).ToList();
            model.OonHistory = new List<McpdOutOfNetwork>();
            model.OonError = new List<McpdOutOfNetwork>();
            var result = _oonController.Index(991, model);
            Assert.IsTrue(result.IsCompleted);
        }
    }
}
