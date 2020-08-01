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
    public class LogUnitTest
    {
        private LogContext _contextLog;

        private HomeController _logController;

        [TestInitialize]
        public void Setup()
        {
            var optionl = new DbContextOptionsBuilder<LogContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            _contextLog = new LogContext(optionl);
            _logController = new HomeController(_contextLog);
        }
        [TestMethod]
        public void TestLogController()
        {
            DashboardViewModel model = new DashboardViewModel();
            model.ProcessLogs = new List<ProcessLogDisplayModel>()
            {
                new ProcessLogDisplayModel
                {
                     AppealErrors=0,
                      AppealSubmits=10,
                       AppealTotal=10,
                        COCErrors=0,
                         COCSubmits=10,
                          COCTotal=10,
                           GrievanceErrors=0,
                            GrievanceSubmits=10,
                             GrievanceTotal=10,
                              OONErrors=0,
                               OONSubmits=10,
                                OONTotal=10,
                                 PCPAErrors=0,
                                  PCPASubmits=10,
                                   PCPATotal=10,
                                    RecordMonth="02",
                                     RecordYear="2020",
                                      RunBy="I8378",
                                       RunStatus="0",
                                        RunTime=DateTime.Now,
                                         TradingPartnerCode="IEHP"
                },
                new ProcessLogDisplayModel
                {
                     AppealErrors=0,
                      AppealSubmits=10,
                       AppealTotal=10,
                        COCErrors=0,
                         COCSubmits=10,
                          COCTotal=10,
                           GrievanceErrors=0,
                            GrievanceSubmits=10,
                             GrievanceTotal=10,
                              OONErrors=0,
                               OONSubmits=10,
                                OONTotal=10,
                                 PCPAErrors=0,
                                  PCPASubmits=10,
                                   PCPATotal=10,
                                    RecordMonth="02",
                                     RecordYear="2020",
                                      RunBy="I8378",
                                       RunStatus="0",
                                        RunTime=DateTime.Now,
                                         TradingPartnerCode="Kaiser"
                }
            };
            var result = _logController.Index(991, model) as ViewResult;
            var resultData = result.Model as DashboardViewModel;
            Assert.AreEqual(2, resultData.ProcessLogs.Count);
        }
    }
}

