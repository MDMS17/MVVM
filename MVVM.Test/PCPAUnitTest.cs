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
    public class PCPAUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private PcpaController _pcpaController;

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
            List<PcpAssignment> pcpas = new List<PcpAssignment>()
            {
                new PcpAssignment()
                {
                     PcpAssignmentId = 1,
                     PcpHeaderId=1,
                     PlanCode="305",
                     Cin="32001378A",
                     Npi="1205940145",
                     TradingPartnerCode="IEHP"
                },
                new PcpAssignment()
                {
                    PcpAssignmentId=2,
                    PcpHeaderId=1,
                    PlanCode="305",
                    Cin="32001379A",
                    Npi="1356455299",
                    TradingPartnerCode="IEHP"
                }
            };
            _context.PcpAssignments.AddRange(pcpas);
            _pcpaController = new PcpaController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestPCPAController()
        {
            PcpaViewModel model = new PcpaViewModel();
            model.PcpaCurrent = _context.PcpAssignments.Local.Select(x => new PcpAssignment
            {
                PcpAssignmentId = x.PcpAssignmentId,
                PcpHeaderId = x.PcpHeaderId,
                Cin = x.Cin,
                PlanCode = x.PlanCode,
                Npi = x.Npi,
                TradingPartnerCode = x.TradingPartnerCode,
                ErrorMessage = x.ErrorMessage,
                DataSource = x.DataSource
            }).ToList();
            model.PcpaHistory = new List<PcpAssignment>();
            model.PcpaError = new List<PcpAssignment>();
            var result = _pcpaController.Index(991, model);
            Assert.IsTrue(result.IsCompleted);
        }
    }
}
