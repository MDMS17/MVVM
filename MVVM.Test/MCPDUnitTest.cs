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
    public class MCPDUnitTest
    {
        private StagingContext _context;
        private HistoryContext _contextHistory;
        private ErrorContext _contextError;
        private LogContext _contextLog;

        private JsonController _jsonController;

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
            _jsonController = new JsonController(_context, _contextHistory, _contextError, _contextLog);
        }
        [TestMethod]
        public void TestMCPDJsonGeneration()
        {
            McpdViewModel model = new McpdViewModel();
            model.mcpdHeader = new McpdHeader()
            {
                McpdHeaderId = 1,
                PlanParent = "IEHP",
                ReportingPeriod = "202002",
                SchemaVersion = "1.3",
                SubmissionDate = DateTime.Today
            };
            var result = _jsonController.McpdJson(991, model) as ViewResult;
            var resultData = result.Model as McpdViewModel;
            Assert.AreEqual(resultData.mcpdHeader.SchemaVersion, "1.3");
        }
    }
}

