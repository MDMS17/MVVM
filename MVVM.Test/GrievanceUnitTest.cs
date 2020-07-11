using Microsoft.VisualStudio.TestTools.UnitTesting;
using mcpdandpcpa.Controllers;
using mcpdandpcpa.Models;
using mcpdipData;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace MCPDIP.Test
{
    [TestClass]
    public class GrievanceUnitTest
    {
        private Mock<StagingContext> _context;
        private Mock<HistoryContext> _contextHistory;
        private Mock<ErrorContext> _contextError;
        private Mock<LogContext> _contextLog;

        private GrievanceController _grievanceController;

        [TestInitialize]
        public void Setup()
        {
            _context = new Mock<StagingContext>();
            _contextHistory = new Mock<HistoryContext>();
            _contextError = new Mock<ErrorContext>();
            _contextLog = new Mock<LogContext>();
            _grievanceController = new GrievanceController(_context.Object, _contextHistory.Object, _contextError.Object, _contextLog.Object);
        }
        [TestMethod]
        public void TestGrievanceController()
        {
            var result = _grievanceController.Index();

        }
        [TestMethod]
        public void TestGrievanceStagingPagination()
        {
        }
        [TestMethod]
        public void TestGrievanceHistoryPagination()
        {
        }
        [TestMethod]
        public void TestGrievanceErrorPagination()
        {
        }
        [TestMethod]
        public void TestGrievanceStagingDownload()
        {
        }
        [TestMethod]
        public void TestGrievanceHistoryDownload()
        {
        }
        [TestMethod]
        public void TestGrievanceErrorDownload()
        {
        }
    }
}

