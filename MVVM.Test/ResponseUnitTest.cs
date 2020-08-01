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
    public class ResponseUnitTest
    {
        private ResponseContext _context;
        private HistoryContext _contextHistory;
        private LogContext _contextLog;
        private StagingContext _contextStaging;

        private ResponseController _responseController;

        [TestInitialize]
        public void Setup()
        {
            var optionr = new DbContextOptionsBuilder<ResponseContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var optionh = new DbContextOptionsBuilder<HistoryContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var optionl = new DbContextOptionsBuilder<LogContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var options = new DbContextOptionsBuilder<StagingContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _context = new ResponseContext(optionr);
            _contextHistory = new HistoryContext(optionh);
            _contextLog = new LogContext(optionl);
            _contextStaging = new StagingContext(options);
            _responseController = new ResponseController(_context, _contextHistory, _contextLog, _contextStaging);
        }
        [TestMethod]
        public void TestResponseController()
        {
            ResponseViewModel model = new ResponseViewModel();
            model.Message = "This is a test";
            var result = _responseController.LoadResponse(991, model) as ViewResult;
            var resultData = result.Model as ResponseViewModel;
            Assert.AreEqual(resultData.Message, "This is a test");
        }
    }
}

