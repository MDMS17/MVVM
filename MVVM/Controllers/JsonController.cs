using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using mcpdandpcpa.Models;
using mcpdipData;
using JsonLib;
using mcpdandpcpa.Extensions;
using Microsoft.AspNetCore.Authorization;


namespace mcpdandpcpa.Controllers
{
    public class JsonController : Controller
    {
        private readonly StagingContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly ErrorContext _contextError;
        private readonly LogContext _contextLog;
        public JsonController(StagingContext context, HistoryContext contextHistory, ErrorContext contextError, LogContext contextLog)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextError = contextError;
            _contextLog = contextLog;
        }
        public async Task<IActionResult> MCPDJson()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            McpdViewModel model = new McpdViewModel();

            model.JsonExportPath = configuration["JsonExportPath"];
            McpdHeader mcpdHeader = await _context.McpdHeaders.FirstOrDefaultAsync();
            if (string.IsNullOrEmpty(mcpdHeader.ReportingPeriod)) mcpdHeader.ReportingPeriod = DateTime.Today.AddMonths(-1).ToString("yyyyMMdd");
            await _context.SaveChangesAsync();
            mcpdHeader.SubmissionDate = DateTime.Today;
            model.mcpdHeader = mcpdHeader;
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> MCPDJson(McpdViewModel model)
        {
            model.mcpdHeader = await _context.McpdHeaders.FirstOrDefaultAsync();
            model.mcpdHeader.SubmissionDate = DateTime.Today;
            if (model.SelectedJsonFileMode == "Production")
            {
                //check processlog, if someone else is working on the same task
                ProcessLog processLog = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.RunStatus == "1");
                if (processLog != null)
                {
                    model.mcpdJsonMessage = $"Same task has been initialized by {processLog.RunBy }";
                    return View(model);
                }
                else
                {
                    foreach (string IPA in model.Ipas.Except(new List<string>() { "All" }))
                    {
                        processLog = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.TradingPartnerCode == IPA && x.RecordYear == model.mcpdHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.mcpdHeader.ReportingPeriod.Substring(4, 2));
                        if (processLog == null)
                        {
                            processLog = new ProcessLog
                            {
                                TradingPartnerCode = IPA,
                                RecordYear = model.mcpdHeader.ReportingPeriod.Substring(0, 4),
                                RecordMonth = model.mcpdHeader.ReportingPeriod.Substring(4, 2),
                                RunStatus = "1",
                                RunTime = DateTime.Now,
                                RunBy = User.Identity.Name
                            };
                            _contextLog.Add(processLog);
                        }
                        else
                        {
                            processLog.RunStatus = "1";
                            processLog.RunTime = DateTime.Now;
                            processLog.RunBy = User.Identity.Name;
                        }
                        await _contextLog.SaveChangesAsync();
                    }
                }
                McpdHeader headerHistory = await _contextHistory.McpdHeaders.FirstOrDefaultAsync(x => x.ReportingPeriod.Substring(0, 6) == model.mcpdHeader.ReportingPeriod.Substring(0, 6));
                if (headerHistory != null)
                {
                    _contextHistory.Grievances.RemoveRange(_contextHistory.Grievances.Where(x => x.McpdHeaderId == headerHistory.McpdHeaderId));
                    _contextHistory.Appeals.RemoveRange(_contextHistory.Appeals.Where(x => x.McpdHeaderId == headerHistory.McpdHeaderId));
                    _contextHistory.McpdContinuityOfCare.RemoveRange(_contextHistory.McpdContinuityOfCare.Where(x => x.McpdHeaderId == headerHistory.McpdHeaderId));
                    _contextHistory.McpdOutOfNetwork.RemoveRange(_contextHistory.McpdOutOfNetwork.Where(x => x.McpdHeaderId == headerHistory.McpdHeaderId));
                    await _contextHistory.SaveChangesAsync();
                    _contextHistory.McpdHeaders.Remove(headerHistory);
                    await _contextHistory.SaveChangesAsync();
                }
                McpdHeader headerError = await _contextError.McpdHeaders.FirstOrDefaultAsync(x => x.ReportingPeriod.Substring(0, 6) == model.mcpdHeader.ReportingPeriod.Substring(0, 6));
                if (headerError != null)
                {
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdGrievance where McpdHeaderId={headerError.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdAppeal where McpdHeaderId={headerError.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdContinuityOfCare where McpdHeaderId={headerError.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdOutOfNetwork where McpdHeaderId={headerError.McpdHeaderId.ToString()}");
                    _contextError.McpdHeaders.Remove(headerError);
                    await _contextError.SaveChangesAsync();
                }
                //check schema for grievances, add error to errorcontext, add valid to historycontext
                List<McpdGrievance> allGrievances = await _context.Grievances.ToListAsync();
                List<Tuple<string, bool, string>> grievanceSchemas = JsonOperations.ValidateGrievance(allGrievances);
                List<McpdGrievance> validGrievances = new List<McpdGrievance>();
                List<McpdGrievance> errorGrievances = new List<McpdGrievance>();
                List<string> dupGrievanceIds = _context.Grievances.GroupBy(x => x.GrievanceId).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                string maxReceiveDate = DateTime.Today.ToString("yyyyMM") + "01";
                foreach (McpdGrievance grievance in allGrievances)
                {
                    bool hasError = false;
                    grievance.ErrorMessage = "";
                    if (!grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item2)
                    {
                        grievance.ErrorMessage = "Schema Error:" + grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item3;
                        hasError = true;
                    }
                    else
                    {
                        //BL_Grievance001
                        if (dupGrievanceIds.Contains(grievance.GrievanceId))
                        {
                            grievance.ErrorMessage += "Business Error: Duplicated Grievance Id~";
                            hasError = true;
                        }
                        //BL_Grievance002
                        if (grievance.GrievanceId.Substring(0, 3) != grievance.PlanCode)
                        {
                            grievance.ErrorMessage += "Business Error: grievance id should start with plan code~";
                            hasError = true;
                        }
                        //BL_Grievance003
                        if (string.Compare(grievance.GrievanceReceivedDate, maxReceiveDate) >= 0)
                        {
                            grievance.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                            hasError = true;
                        }
                        //BL_Grievance004
                        if (grievance.RecordType == "Original" && !string.IsNullOrEmpty(grievance.ParentGrievanceId))
                        {
                            grievance.ErrorMessage += "Business Error: Parent grievance id not allowed for Original~";
                            hasError = true;
                        }
                        if (grievance.RecordType != "Original")
                        {
                            if (string.IsNullOrEmpty(grievance.ParentGrievanceId))
                            {
                                grievance.ErrorMessage += "Business Error: Parent grievance id is missing for non Original~";
                                hasError = true;
                            }
                            else
                            {
                                var parentGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.GrievanceId == grievance.ParentGrievanceId);
                                if (parentGrievance == null)
                                {
                                    grievance.ErrorMessage += "Business Error: Parent grievance id couldnot be found~";
                                    hasError = true;
                                }
                                else
                                {
                                    var processedGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.ParentGrievanceId == grievance.ParentGrievanceId);
                                    if (processedGrievance != null)
                                    {
                                        grievance.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                        hasError = true;
                                    }
                                }
                            }
                        }
                    }
                    if (hasError)
                    {
                        if (grievance.ErrorMessage.Length > 255) grievance.ErrorMessage = grievance.ErrorMessage.Substring(0, 255);
                        errorGrievances.Add(grievance);
                    }
                    else
                    {
                        validGrievances.Add(grievance);
                    }
                }
                List<McpdAppeal> allAppeals = await _context.Appeals.ToListAsync();
                List<Tuple<string, bool, string>> appealSchemas = JsonOperations.ValidateAppeal(allAppeals);
                List<McpdAppeal> validAppeals = new List<McpdAppeal>();
                List<McpdAppeal> errorAppeals = new List<McpdAppeal>();
                List<string> dupAppealIds = _context.Appeals.GroupBy(x => x.AppealId).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                foreach (McpdAppeal appeal in allAppeals)
                {
                    bool hasError = false;
                    appeal.ErrorMessage = "";
                    if (!appealSchemas.First(x => x.Item1 == appeal.AppealId).Item2)
                    {
                        appeal.ErrorMessage = "Schema Error: " + appealSchemas.First(x => x.Item1 == appeal.AppealId).Item3;
                        hasError = true;
                    }
                    else
                    {
                        //BL_Appeal001
                        if (dupAppealIds.Contains(appeal.AppealId))
                        {
                            appeal.ErrorMessage += "Business Error: Duplicated appeal id~";
                            hasError = true;
                        }
                        //BL_Appeal002
                        if (appeal.AppealId.Substring(0, 3) != appeal.PlanCode)
                        {
                            appeal.ErrorMessage += "Business Error: Appeal id should start with plan code~";
                            hasError = true;
                        }
                        //BL_Appeal003
                        if (string.Compare(appeal.AppealReceivedDate, maxReceiveDate) >= 0)
                        {
                            appeal.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                            hasError = true;
                        }
                        //BL_Appeal004
                        if (string.Compare(appeal.NoticeOfActionDate, maxReceiveDate) >= 0)
                        {
                            appeal.ErrorMessage += "Business Error: Action date should be prior to current month~";
                            hasError = true;
                        }
                        //BL_Appeal005
                        if (appeal.RecordType == "Original" && !string.IsNullOrEmpty(appeal.ParentAppealId))
                        {
                            appeal.ErrorMessage += "Business Error: Parent appeal id not allowed for Original~";
                            hasError = true;
                        }
                        if (appeal.RecordType != "Original")
                        {
                            if (string.IsNullOrEmpty(appeal.ParentAppealId))
                            {
                                appeal.ErrorMessage += "Business Error: Parent appeal id is missing for non Original~";
                                hasError = true;
                            }
                            else
                            {
                                var parentAppeal = _contextHistory.Appeals.FirstOrDefault(x => x.AppealId == appeal.ParentAppealId);
                                if (parentAppeal == null)
                                {
                                    appeal.ErrorMessage += "Business Error: Parent appeal id couldnot be found~";
                                    hasError = true;
                                }
                                else
                                {
                                    var processedAppeal = _contextHistory.Appeals.FirstOrDefault(x => x.ParentAppealId == appeal.ParentAppealId);
                                    if (processedAppeal != null)
                                    {
                                        appeal.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                        hasError = true;
                                    }
                                }
                            }
                        }
                        //BL_Appeal006
                        if (appeal.AppealResolutionStatusIndicator == "Unresolved")
                        {
                            if (!string.IsNullOrEmpty(appeal.AppealResolutionDate))
                            {
                                appeal.ErrorMessage += "Business Error: Resolution date not allowed for unresolved appeal~";
                                hasError = true;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(appeal.AppealResolutionDate))
                            {
                                appeal.ErrorMessage += "Business Error: Resolution date must be populated for resolved appeal~";
                                hasError = true;
                            }
                            else
                            {
                                if (string.Compare(appeal.AppealResolutionDate, maxReceiveDate) >= 0)
                                {
                                    appeal.ErrorMessage += "Business Error: Resolution date should be prior to current month~";
                                    hasError = true;
                                }
                                if (string.Compare(appeal.AppealResolutionDate, appeal.AppealReceivedDate) < 0)
                                {
                                    appeal.ErrorMessage += "Business Error: Resolution date cannot be prior to receive date~";
                                    hasError = true;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(appeal.ParentGrievanceId))
                        {
                            var appealParentGrievance = _contextHistory.Grievances.FirstOrDefault(x => x.GrievanceId == appeal.ParentGrievanceId);
                            if (appealParentGrievance == null)
                            {
                                appeal.ParentGrievanceId = null;
                            }
                        }
                    }
                    if (hasError)
                    {
                        if (appeal.ErrorMessage.Length > 255) appeal.ErrorMessage = appeal.ErrorMessage.Substring(0, 255);
                        errorAppeals.Add(appeal);
                    }
                    else
                    {
                        validAppeals.Add(appeal);
                    }
                }

                List<McpdContinuityOfCare> allCocs = await _context.McpdContinuityOfCare.ToListAsync();
                List<Tuple<string, bool, string>> cocSchemas = JsonOperations.ValidateCOC(allCocs);
                List<McpdContinuityOfCare> validCocs = new List<McpdContinuityOfCare>();
                List<McpdContinuityOfCare> errorCocs = new List<McpdContinuityOfCare>();
                List<string> dupCocIds = _context.McpdContinuityOfCare.GroupBy(x => x.CocId).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                foreach (McpdContinuityOfCare coc in allCocs)
                {
                    bool hasError = false;
                    coc.ErrorMessage = "";
                    if (!cocSchemas.First(x => x.Item1 == coc.CocId).Item2)
                    {
                        coc.ErrorMessage = "Schem Error: " + cocSchemas.First(x => x.Item1 == coc.CocId).Item3;
                        hasError = true;
                    }
                    else
                    {
                        //BL_Coc001
                        if (dupCocIds.Contains(coc.CocId))
                        {
                            coc.ErrorMessage += "Business Error: Duplicated COC id~";
                            hasError = true;
                        }
                        //BL_Coc002
                        if (coc.CocId.Substring(0, 3) != coc.PlanCode)
                        {
                            coc.ErrorMessage += "Business Error: COC id should start with plan code~";
                            hasError = true;
                        }
                        //BL_Coc003
                        if (string.Compare(coc.CocReceivedDate, maxReceiveDate) >= 0)
                        {
                            coc.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                            hasError = true;
                        }
                        //BL_Coc004
                        if (coc.RecordType == "Original" && !string.IsNullOrEmpty(coc.ParentCocId))
                        {
                            coc.ErrorMessage += "Business Error: Parent COC id not allowed for Original~";
                            hasError = true;
                        }
                        if (coc.RecordType != "Original")
                        {
                            if (string.IsNullOrEmpty(coc.ParentCocId))
                            {
                                coc.ErrorMessage += "Business Error: Parent COC id is missing for non Original~";
                                hasError = true;
                            }
                            else
                            {
                                var parentCoc = _contextHistory.McpdContinuityOfCare.FirstOrDefault(x => x.CocId == coc.ParentCocId);
                                if (parentCoc == null)
                                {
                                    coc.ErrorMessage += "Business Error: Parent COC id couldnot be found~";
                                    hasError = true;
                                }
                                else
                                {
                                    var processedCoc = _contextHistory.McpdContinuityOfCare.FirstOrDefault(x => x.ParentCocId == coc.ParentCocId);
                                    if (processedCoc != null)
                                    {
                                        coc.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                        hasError = true;
                                    }
                                }
                            }
                        }
                        //BL_Coc005
                        if (coc.CocType != "MER Denial" && coc.CocDispositionIndicator == "Provider is in MCP Network")
                        {
                            coc.ErrorMessage += "Business Error: COC disposition indicator must <> Provider is in MCP Network, if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc006
                        if (coc.CocDispositionIndicator == "Denied" && !string.IsNullOrEmpty(coc.CocExpirationDate))
                        {
                            coc.ErrorMessage += "Business Error:  Expiration date must be blank if COC disposition indicator = Denied~";
                            hasError = true;
                        }
                        //BL_Coc007
                        if (coc.CocDispositionIndicator == "Approved" && string.IsNullOrEmpty(coc.CocExpirationDate))
                        {
                            coc.ErrorMessage += "Business Error: Expiration date must be populated if COC disposition indicator = Denied~";
                            hasError = true;
                        }
                        //BL_Coc008
                        if (coc.CocDispositionIndicator == "Denied" && string.IsNullOrEmpty(coc.CocDenialReasonIndicator))
                        {
                            coc.ErrorMessage += "Business Error: COC denial reason indicator must be populated if COC disposition indicator = Denied~";
                            hasError = true;
                        }
                        //BL_Coc009
                        if (coc.CocDispositionIndicator != "Denied" && !string.IsNullOrEmpty(coc.CocDenialReasonIndicator))
                        {
                            coc.ErrorMessage += "Business Error: COC denial reason indicator must be blank if COC type <> Denied~";
                            hasError = true;
                        }
                        //BL_Coc010
                        if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerExemptionId))
                        {
                            coc.ErrorMessage += "Business Error: MER exemption id must be populated if COC type = MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc011
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerExemptionId))
                        {
                            coc.ErrorMessage += "Business Error: MER excemption id must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc012
                        if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialCode))
                        {
                            coc.ErrorMessage += "Business Error: Exemption to enrollment denial code must be populated if COC type = MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc013
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialCode))
                        {
                            coc.ErrorMessage += "Business Error: Exemption to enrollment denial code must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc014
                        if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialDate))
                        {
                            coc.ErrorMessage += "Business Error: Excemption to enrollment denial date must be populated if COC type = MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc015
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ExemptionToEnrollmentDenialDate))
                        {
                            coc.ErrorMessage += "Business Error: Exemption to enrollment denial date must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc016
                        if (coc.CocType == "MER Denial" && string.Compare(coc.ExemptionToEnrollmentDenialDate, maxReceiveDate) >= 0)
                        {
                            coc.ErrorMessage += "Business Error: Exemption to enrollment denial date must be prior to current month~";
                            hasError = true;
                        }
                        //BL_Coc017
                        if (coc.MerCocDispositionIndicator != "MER COC Not Met" && coc.CocProviderNpi != coc.SubmittingProviderNpi)
                        {
                            coc.ErrorMessage += "Business Error: COC provider NPI must = submitting provider NPI, if MER COC disposition indicator <> MER COC Not Met~";
                            hasError = true;
                        }
                        //BL_Coc018
                        if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerCocDispositionIndicator))
                        {
                            coc.ErrorMessage += "Business Error: MER COC disposition indicator must be populated, if COC type = MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc019
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerCocDispositionIndicator))
                        {
                            coc.ErrorMessage += "Business Error: MER COC disposition indicator must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc020
                        if (coc.CocType == "MER Denial" && string.IsNullOrEmpty(coc.MerCocDispositionDate))
                        {
                            coc.ErrorMessage += "Business Error: MER COC disposition date must be populated if COC type = MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc021
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.MerCocDispositionDate))
                        {
                            coc.ErrorMessage += "Business Error: MER COC disposition date must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc022
                        if (coc.CocType == "MER Denial" && string.Compare(coc.MerCocDispositionDate, maxReceiveDate) >= 0)
                        {
                            coc.ErrorMessage += "Business Error: MER COC disposition date must be prior to current month~";
                            hasError = true;
                        }
                        //BL_Coc023
                        if (coc.CocType == "MER Denial" && coc.MerCocDispositionIndicator == "MER COC Not Met" && string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                        {
                            coc.ErrorMessage += "Business Error: Reason MER COC not met must be populated if COC type = MER Denial and MER COC disposition indicator = MER COC Not Met~";
                            hasError = true;
                        }
                        //BL_Coc024
                        if (coc.CocType != "MER Denial" && !string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                        {
                            coc.ErrorMessage += "Business Error: Reason MER COC not met must be blank if COC type <> MER Denial~";
                            hasError = true;
                        }
                        //BL_Coc025
                        if (coc.MerCocDispositionIndicator != "MER COC Not Met" && !string.IsNullOrEmpty(coc.ReasonMerCocNotMetIndicator))
                        {
                            coc.ErrorMessage += "Business Error: Reason MER COC not met must be blank if MER COC disposition indicator <> MER COC Not Met~";
                            hasError = true;
                        }
                    }
                    if (hasError)
                    {
                        if (coc.ErrorMessage.Length > 255) coc.ErrorMessage = coc.ErrorMessage.Substring(0, 255);
                        errorCocs.Add(coc);
                    }
                    else
                    {
                        validCocs.Add(coc);
                    }
                }

                List<McpdOutOfNetwork> allOons = await _context.McpdOutOfNetwork.ToListAsync();
                List<Tuple<string, bool, string>> oonSchemas = JsonOperations.ValidateOON(allOons);
                List<McpdOutOfNetwork> validOons = new List<McpdOutOfNetwork>();
                List<McpdOutOfNetwork> errorOons = new List<McpdOutOfNetwork>();
                List<string> dupOonIds = _context.McpdOutOfNetwork.GroupBy(x => x.OonId).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                foreach (McpdOutOfNetwork oon in allOons)
                {
                    bool hasError = false;
                    oon.ErrorMessage = "";
                    if (!oonSchemas.First(x => x.Item1 == oon.OonId).Item2)
                    {
                        oon.ErrorMessage = "Schem Error: " + oonSchemas.First(x => x.Item1 == oon.OonId).Item3;
                        hasError = true;
                    }
                    else
                    {
                        //BL_Oon001
                        if (dupOonIds.Contains(oon.OonId))
                        {
                            oon.ErrorMessage += "Business Error: Duplicated Oon id~";
                            hasError = true;
                        }
                        //BL_Oon002
                        if (oon.OonId.Substring(0, 3) != oon.PlanCode)
                        {
                            oon.ErrorMessage += "Business Error: Oon id should start with plan code~";
                            hasError = true;
                        }
                        //BL_Oon003
                        if (string.Compare(oon.OonRequestReceivedDate, maxReceiveDate) >= 0)
                        {
                            oon.ErrorMessage += "Business Error: Receive date should be prior to current month~";
                            hasError = true;
                        }
                        //BL_Oon004
                        if (oon.RecordType == "Original" && !string.IsNullOrEmpty(oon.ParentOonId))
                        {
                            oon.ErrorMessage += "Business Error: Parent Oon id not allowed for Original~";
                            hasError = true;
                        }
                        if (oon.RecordType != "Original")
                        {
                            if (string.IsNullOrEmpty(oon.ParentOonId))
                            {
                                oon.ErrorMessage += "Business Error: Parent Oon id is missing for non Original~";
                                hasError = true;
                            }
                            else
                            {
                                var parentOon = _contextHistory.McpdOutOfNetwork.FirstOrDefault(x => x.OonId == oon.ParentOonId);
                                if (parentOon == null)
                                {
                                    oon.ErrorMessage += "Business Error: Parent Oon id couldnot be found~";
                                    hasError = true;
                                }
                                else
                                {
                                    var processedOon = _contextHistory.McpdOutOfNetwork.FirstOrDefault(x => x.ParentOonId == oon.ParentOonId);
                                    if (processedOon != null)
                                    {
                                        oon.ErrorMessage += "Business Error: Already processed before, no more actions~";
                                        hasError = true;
                                    }
                                }
                            }
                        }
                        //BL_Oon005
                        if (oon.OonResolutionStatusIndicator == "Partial Approval" && string.IsNullOrEmpty(oon.PartialApprovalExplanation))
                        {
                            oon.ErrorMessage += "Business Error: Partial Approval Explanation must be populated when OON Resolution Status Indicator = Partial Approval~";
                            hasError = true;
                        }
                        //BL_Oon006
                        if (oon.OonResolutionStatusIndicator == "Pending" && !string.IsNullOrEmpty(oon.OonRequestResolvedDate))
                        {
                            oon.ErrorMessage += "Business Error: OON Request Resolved Date must be blank if OON Resolution Status Indicator = Pending~";
                            hasError = true;
                        }
                        //BL_Oon007
                        if (oon.OonResolutionStatusIndicator != "Pending" && string.IsNullOrEmpty(oon.OonRequestResolvedDate))
                        {
                            oon.ErrorMessage += "Business Error: OON Request Resolved Date must be populated if OON Resolution Status Indicator <> Pending~";
                            hasError = true;
                        }
                        //BL_Oon008
                        if (oon.OonResolutionStatusIndicator != "Pending" && string.Compare(oon.OonRequestResolvedDate, maxReceiveDate) >= 0)
                        {
                            oon.ErrorMessage += "Business Error: OON Request Resolved Date is not a past date~";
                            hasError = true;
                        }
                        //BL_Oon009
                        if (oon.OonResolutionStatusIndicator != "Pending" && string.Compare(oon.OonRequestResolvedDate, oon.OonRequestReceivedDate) < 0)
                        {
                            oon.ErrorMessage += "Business Error: OON Request Resolved Date must be >= OON Request Received Date~";
                            hasError = true;
                        }
                        //BL_OON010
                        if (oon.ServiceLocationCountry != "US" && !string.IsNullOrEmpty(oon.ServiceLocationState))
                        {
                            oon.ErrorMessage += "Business ERror: Foreign country, state should be blank~";
                            hasError = true;
                        }
                        //BL_OON011
                        if (oon.ServiceLocationCountry != "US" && !string.IsNullOrEmpty(oon.ServiceLocationZip))
                        {
                            oon.ErrorMessage += "Business Error: Foreigh country, zip should be blank~";
                            hasError = true;
                        }
                    }
                    if (hasError)
                    {
                        if (oon.ErrorMessage.Length > 255) oon.ErrorMessage = oon.ErrorMessage.Substring(0, 255);
                        errorOons.Add(oon);
                    }
                    else
                    {
                        validOons.Add(oon);
                    }
                }

                McpdHeader mcpdHeaderHistory = new McpdHeader
                {
                    PlanParent = model.mcpdHeader.PlanParent,
                    SubmissionDate = model.mcpdHeader.SubmissionDate,
                    SchemaVersion = model.mcpdHeader.SchemaVersion,
                    ReportingPeriod = model.mcpdHeader.ReportingPeriod
                };
                _contextHistory.McpdHeaders.Add(mcpdHeaderHistory);
                await _contextHistory.SaveChangesAsync();
                _contextHistory.Grievances.AddRange(validGrievances.Select(x => new McpdGrievance
                {
                    McpdHeaderId = mcpdHeaderHistory.McpdHeaderId,
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    GrievanceId = x.GrievanceId,
                    RecordType = x.RecordType,
                    ParentGrievanceId = x.ParentGrievanceId,
                    GrievanceReceivedDate = x.GrievanceReceivedDate,
                    GrievanceType = x.GrievanceType,
                    BenefitType = x.BenefitType,
                    ExemptIndicator = x.ExemptIndicator,
                    TradingPartnerCode = x.TradingPartnerCode,
                    DataSource = x.DataSource,
                    CaseNumber = x.CaseNumber,
                    CaseStatus = x.CaseStatus
                }));
                _contextHistory.Appeals.AddRange(validAppeals.Select(x => new McpdAppeal
                {
                    McpdHeaderId = mcpdHeaderHistory.McpdHeaderId,
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    AppealId = x.AppealId,
                    RecordType = x.RecordType,
                    ParentGrievanceId = x.ParentGrievanceId,
                    ParentAppealId = x.ParentAppealId,
                    AppealReceivedDate = x.AppealReceivedDate,
                    NoticeOfActionDate = x.NoticeOfActionDate,
                    AppealType = x.AppealType,
                    BenefitType = x.BenefitType,
                    AppealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                    AppealResolutionDate = x.AppealResolutionDate,
                    PartiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                    ExpeditedIndicator = x.ExpeditedIndicator,
                    TradingPartnerCode = x.TradingPartnerCode,
                    DataSource = x.DataSource,
                    CaseNumber = x.CaseNumber,
                    CaseStatus = x.CaseStatus
                }));
                _contextHistory.McpdContinuityOfCare.AddRange(validCocs.Select(x => new McpdContinuityOfCare
                {
                    McpdHeaderId = mcpdHeaderHistory.McpdHeaderId,
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
                    TradingPartnerCode = x.TradingPartnerCode,
                    DataSource = x.DataSource,
                    CaseNumber = x.CaseNumber,
                    CaseStatus = x.CaseStatus
                }));
                _contextHistory.McpdOutOfNetwork.AddRange(validOons.Select(x => new McpdOutOfNetwork
                {
                    McpdHeaderId = mcpdHeaderHistory.McpdHeaderId,
                    PlanCode = x.PlanCode,
                    Cin = x.Cin,
                    OonId = x.OonId,
                    RecordType = x.RecordType,
                    ParentOonId = x.ParentOonId,
                    OonRequestReceivedDate = x.OonRequestReceivedDate,
                    ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                    OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                    OonRequestResolvedDate = x.OonRequestResolvedDate,
                    PartialApprovalExplanation = x.PartialApprovalExplanation,
                    SpecialistProviderNpi = x.SpecialistProviderNpi,
                    ProviderTaxonomy = x.ProviderTaxonomy,
                    ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                    ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                    ServiceLocationCity = x.ServiceLocationCity,
                    ServiceLocationState = x.ServiceLocationState,
                    ServiceLocationZip = x.ServiceLocationZip,
                    ServiceLocationCountry = x.ServiceLocationCountry,
                    TradingPartnerCode = x.TradingPartnerCode,
                    DataSource = x.DataSource,
                    CaseNumber = x.CaseNumber,
                    CaseStatus = x.CaseStatus
                }));
                await _contextHistory.SaveChangesAsync();
                if (errorGrievances.Count() > 0 || errorAppeals.Count() > 0 || errorCocs.Count() > 0 || errorOons.Count() > 0)
                {
                    McpdHeader mcpdHeaderError = new McpdHeader
                    {
                        PlanParent = model.mcpdHeader.PlanParent,
                        SubmissionDate = model.mcpdHeader.SubmissionDate,
                        SchemaVersion = model.mcpdHeader.SchemaVersion,
                        ReportingPeriod = model.mcpdHeader.ReportingPeriod
                    };
                    _contextError.McpdHeaders.Add(mcpdHeaderError);
                    await _contextError.SaveChangesAsync();
                    if (errorGrievances.Count() > 0)
                    {
                        _contextError.Grievances.AddRange(errorGrievances.Select(x => new McpdGrievance
                        {
                            McpdHeaderId = mcpdHeaderError.McpdHeaderId,
                            PlanCode = x.PlanCode,
                            Cin = x.Cin,
                            GrievanceId = x.GrievanceId,
                            RecordType = x.RecordType,
                            ParentGrievanceId = x.ParentGrievanceId,
                            GrievanceReceivedDate = x.GrievanceReceivedDate,
                            GrievanceType = x.GrievanceType,
                            BenefitType = x.BenefitType,
                            ExemptIndicator = x.ExemptIndicator,
                            TradingPartnerCode = x.TradingPartnerCode,
                            ErrorMessage = x.ErrorMessage,
                            DataSource = x.DataSource,
                            CaseNumber = x.CaseNumber,
                            CaseStatus = x.CaseStatus
                        }));
                    }
                    if (errorAppeals.Count() > 0)
                    {
                        _contextError.Appeals.AddRange(errorAppeals.Select(x => new McpdAppeal
                        {
                            McpdHeaderId = mcpdHeaderError.McpdHeaderId,
                            PlanCode = x.PlanCode,
                            Cin = x.Cin,
                            AppealId = x.AppealId,
                            RecordType = x.RecordType,
                            ParentGrievanceId = x.ParentGrievanceId,
                            ParentAppealId = x.ParentAppealId,
                            AppealReceivedDate = x.AppealReceivedDate,
                            NoticeOfActionDate = x.NoticeOfActionDate,
                            AppealType = x.AppealType,
                            BenefitType = x.BenefitType,
                            AppealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                            AppealResolutionDate = x.AppealResolutionDate,
                            PartiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                            ExpeditedIndicator = x.ExpeditedIndicator,
                            TradingPartnerCode = x.TradingPartnerCode,
                            ErrorMessage = x.ErrorMessage,
                            DataSource = x.DataSource,
                            CaseNumber = x.CaseNumber,
                            CaseStatus = x.CaseStatus
                        }));
                    }
                    if (errorCocs.Count() > 0)
                    {
                        _contextError.McpdContinuityOfCare.AddRange(errorCocs.Select(x => new McpdContinuityOfCare
                        {
                            McpdHeaderId = mcpdHeaderError.McpdHeaderId,
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
                            TradingPartnerCode = x.TradingPartnerCode,
                            ErrorMessage = x.ErrorMessage,
                            DataSource = x.DataSource,
                            CaseNumber = x.CaseNumber,
                            CaseStatus = x.CaseStatus
                        }));
                    }
                    if (errorOons.Count() > 0)
                    {
                        _contextError.McpdOutOfNetwork.AddRange(errorOons.Select(x => new McpdOutOfNetwork
                        {
                            McpdHeaderId = mcpdHeaderError.McpdHeaderId,
                            PlanCode = x.PlanCode,
                            Cin = x.Cin,
                            OonId = x.OonId,
                            RecordType = x.RecordType,
                            ParentOonId = x.ParentOonId,
                            OonRequestReceivedDate = x.OonRequestReceivedDate,
                            ReferralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                            OonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                            OonRequestResolvedDate = x.OonRequestResolvedDate,
                            PartialApprovalExplanation = x.PartialApprovalExplanation,
                            SpecialistProviderNpi = x.SpecialistProviderNpi,
                            ProviderTaxonomy = x.ProviderTaxonomy,
                            ServiceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                            ServiceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                            ServiceLocationCity = x.ServiceLocationCity,
                            ServiceLocationState = x.ServiceLocationState,
                            ServiceLocationZip = x.ServiceLocationZip,
                            ServiceLocationCountry = x.ServiceLocationCountry,
                            TradingPartnerCode = x.TradingPartnerCode,
                            ErrorMessage = x.ErrorMessage,
                            DataSource = x.DataSource,
                            CaseNumber = x.CaseNumber,
                            CaseStatus = x.CaseStatus
                        }));
                    }
                    await _contextError.SaveChangesAsync();
                }
                JsonMcpd jsonMcpd = new JsonMcpd();
                jsonMcpd.header = new JsonMcpdHeader
                {
                    planParent = model.mcpdHeader.PlanParent,
                    submissionDate = model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd"),
                    schemaVersion = model.mcpdHeader.SchemaVersion
                };
                jsonMcpd.grievances = validGrievances.Select(x => new JsonGrievance
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    grievanceId = x.GrievanceId,
                    recordType = x.RecordType,
                    parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                    grievanceReceivedDate = x.GrievanceReceivedDate,
                    grievanceType = x.GrievanceType.Split('|').ToList(),
                    benefitType = x.BenefitType,
                    exemptIndicator = x.ExemptIndicator
                }).ToList();
                jsonMcpd.appeals = validAppeals.Select(x => new JsonAppeal
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    appealId = x.AppealId,
                    recordType = x.RecordType,
                    parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                    parentAppealId = string.IsNullOrEmpty(x.ParentAppealId) ? null : x.ParentAppealId,
                    appealReceivedDate = x.AppealReceivedDate,
                    noticeOfActionDate = x.NoticeOfActionDate,
                    appealType = x.AppealType,
                    benefitType = x.BenefitType,
                    appealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                    appealResolutionDate = x.AppealResolutionDate,
                    partiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                    expeditedIndicator = x.ExpeditedIndicator
                }).ToList();
                jsonMcpd.continuityOfCare = validCocs.Select(x => new JsonCOC
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    cocId = x.CocId,
                    recordType = x.RecordType,
                    parentCocId = x.ParentCocId,
                    cocReceivedDate = x.CocReceivedDate,
                    cocType = x.CocType,
                    benefitType = x.BenefitType,
                    cocDispositionIndicator = x.CocDispositionIndicator,
                    cocExpirationDate = x.CocExpirationDate,
                    cocDenialReasonIndicator = x.CocDenialReasonIndicator,
                    submittingProviderNpi = x.SubmittingProviderNpi,
                    cocProviderNpi = x.CocProviderNpi,
                    providerTaxonomy = x.ProviderTaxonomy,
                    merExemptionId = x.MerExemptionId,
                    exemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                    exemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                    merCocDispositionIndicator = x.MerCocDispositionIndicator,
                    merCocDispositionDate = x.MerCocDispositionDate,
                    reasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator
                }).ToList();
                jsonMcpd.outOfNetwork = validOons.Select(x => new JsonOON
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    oonId = x.OonId,
                    recordType = x.RecordType,
                    parentOonId = x.ParentOonId,
                    oonRequestReceivedDate = x.OonRequestReceivedDate,
                    referralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                    oonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                    oonRequestResolvedDate = x.OonRequestResolvedDate,
                    partialApprovalExplanation = x.PartialApprovalExplanation,
                    specialistProviderNpi = x.SpecialistProviderNpi,
                    providerTaxonomy = x.ProviderTaxonomy,
                    serviceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                    serviceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                    serviceLocationCity = x.ServiceLocationCity,
                    serviceLocationState = x.ServiceLocationState,
                    serviceLocationZip = x.ServiceLocationZip,
                    serviceLocationCountry = x.ServiceLocationCountry
                }).ToList();
                string fileName = "IEHP_MCPD_" + model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd") + "_" + model.FileVersion + ".json";
                System.IO.File.WriteAllText(Path.Combine(model.JsonExportPath, fileName), JsonOperations.GetMcpdJson(jsonMcpd));
                model.mcpdJsonMessage = $"Json file {fileName} generation in {model.JsonExportPath} completed";
                SubmissionLog log2 = await _contextLog.SubmissionLogs.FirstOrDefaultAsync(x => x.RecordYear == model.mcpdHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.mcpdHeader.ReportingPeriod.Substring(4, 2) && x.FileType == "MCPD");
                if (log2 == null)
                {
                    log2 = new SubmissionLog
                    {
                        RecordYear = model.mcpdHeader.ReportingPeriod.Substring(0, 4),
                        RecordMonth = model.mcpdHeader.ReportingPeriod.Substring(4, 2),
                        FileName = fileName,
                        FileType = "MCPD",
                        SubmitterName = "IEHP",
                        SubmissionDate = model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd"),
                        CreateDate = DateTime.Now,
                        CreateBy = User.Identity.Name
                    };
                    _contextLog.Add(log2);
                    await _contextLog.SaveChangesAsync();
                }
                log2.FileName = fileName;
                log2.SubmissionDate = model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd");
                log2.TotalGrievanceSubmitted = validGrievances.Count();
                log2.TotalAppealSubmitted = validAppeals.Count();
                log2.TotalCOCSubmitted = validCocs.Count();
                log2.TotalOONSubmitted = validOons.Count();
                log2.UpdateDate = DateTime.Now;
                log2.UpdateBy = User.Identity.Name;
                await _contextLog.SaveChangesAsync();
                foreach (string IPA in model.Ipas.Except(new List<string>() { "All" }))
                {
                    ProcessLog log = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.TradingPartnerCode == IPA && x.RecordYear == model.mcpdHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.mcpdHeader.ReportingPeriod.Substring(4, 2));
                    int countValidGrievance = validGrievances.Count(x => x.TradingPartnerCode == IPA);
                    int countErrorGrievance = errorGrievances.Count(x => x.TradingPartnerCode == IPA);
                    int countValidAppeal = validAppeals.Count(x => x.TradingPartnerCode == IPA);
                    int countErrorAppeal = errorAppeals.Count(x => x.TradingPartnerCode == IPA);
                    int countValidCOC = validCocs.Count(x => x.TradingPartnerCode == IPA);
                    int countErrorCOC = errorCocs.Count(x => x.TradingPartnerCode == IPA);
                    int countValidOON = validOons.Count(x => x.TradingPartnerCode == IPA);
                    int countErrorOON = errorOons.Count(x => x.TradingPartnerCode == IPA);
                    log.GrievanceErrors = countErrorGrievance;
                    log.GrievanceSubmits = countValidGrievance;
                    log.GrievanceTotal = countValidGrievance + countErrorGrievance;
                    log.AppealErrors = countErrorAppeal;
                    log.AppealSubmits = countValidAppeal;
                    log.AppealTotal = countValidAppeal + countErrorAppeal;
                    log.COCSubmits = countValidCOC;
                    log.COCErrors = countErrorCOC;
                    log.COCTotal = countValidCOC + countErrorCOC;
                    log.OONSubmits = countValidOON;
                    log.OONErrors = countErrorOON;
                    log.OONTotal = countValidOON + countErrorOON;
                    log.RunStatus = "0";
                    await _contextLog.SaveChangesAsync();
                }
            }
            else
            {
                JsonMcpd jsonMcpd = new JsonMcpd();
                jsonMcpd.header = new JsonMcpdHeader
                {
                    planParent = model.mcpdHeader.PlanParent,
                    submissionDate = model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd"),
                    schemaVersion = model.mcpdHeader.SchemaVersion
                };
                jsonMcpd.grievances = _context.Grievances.Select(x => new JsonGrievance
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    grievanceId = x.GrievanceId,
                    recordType = x.RecordType,
                    parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                    grievanceReceivedDate = x.GrievanceReceivedDate,
                    grievanceType = x.GrievanceType.Split(new char[] { '|' }).ToList(),
                    benefitType = x.BenefitType,
                    exemptIndicator = x.ExemptIndicator
                }).ToList();
                jsonMcpd.appeals = _context.Appeals.Select(x => new JsonAppeal
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    appealId = x.AppealId,
                    recordType = x.RecordType,
                    parentGrievanceId = string.IsNullOrEmpty(x.ParentGrievanceId) ? null : x.ParentGrievanceId,
                    parentAppealId = string.IsNullOrEmpty(x.ParentAppealId) ? null : x.ParentAppealId,
                    appealReceivedDate = x.AppealReceivedDate,
                    noticeOfActionDate = x.NoticeOfActionDate,
                    appealType = x.AppealType,
                    benefitType = x.BenefitType,
                    appealResolutionStatusIndicator = x.AppealResolutionStatusIndicator,
                    appealResolutionDate = x.AppealResolutionDate,
                    partiallyOverturnIndicator = x.PartiallyOverturnIndicator,
                    expeditedIndicator = x.ExpeditedIndicator
                }).ToList();
                jsonMcpd.continuityOfCare = _context.McpdContinuityOfCare.Select(x => new JsonCOC
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    cocId = x.CocId,
                    recordType = x.RecordType,
                    parentCocId = x.ParentCocId,
                    cocReceivedDate = x.CocReceivedDate,
                    cocType = x.CocType,
                    benefitType = x.BenefitType,
                    cocDispositionIndicator = x.CocDispositionIndicator,
                    cocExpirationDate = x.CocExpirationDate,
                    cocDenialReasonIndicator = x.CocDenialReasonIndicator,
                    submittingProviderNpi = x.SubmittingProviderNpi,
                    cocProviderNpi = x.CocProviderNpi,
                    providerTaxonomy = x.ProviderTaxonomy,
                    merExemptionId = x.MerExemptionId,
                    exemptionToEnrollmentDenialCode = x.ExemptionToEnrollmentDenialCode,
                    exemptionToEnrollmentDenialDate = x.ExemptionToEnrollmentDenialDate,
                    merCocDispositionIndicator = x.MerCocDispositionIndicator,
                    merCocDispositionDate = x.MerCocDispositionDate,
                    reasonMerCocNotMetIndicator = x.ReasonMerCocNotMetIndicator
                }).ToList();
                jsonMcpd.outOfNetwork = _context.McpdOutOfNetwork.Select(x => new JsonOON
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    oonId = x.OonId,
                    recordType = x.RecordType,
                    parentOonId = x.ParentOonId,
                    oonRequestReceivedDate = x.OonRequestReceivedDate,
                    referralRequestReasonIndicator = x.ReferralRequestReasonIndicator,
                    oonResolutionStatusIndicator = x.OonResolutionStatusIndicator,
                    oonRequestResolvedDate = x.OonRequestResolvedDate,
                    partialApprovalExplanation = x.PartialApprovalExplanation,
                    specialistProviderNpi = x.SpecialistProviderNpi,
                    providerTaxonomy = x.ProviderTaxonomy,
                    serviceLocationAddressLine1 = x.ServiceLocationAddressLine1,
                    serviceLocationAddressLine2 = x.ServiceLocationAddressLine2,
                    serviceLocationCity = x.ServiceLocationCity,
                    serviceLocationState = x.ServiceLocationState,
                    serviceLocationZip = x.ServiceLocationZip,
                    serviceLocationCountry = x.ServiceLocationCountry
                }).ToList();
                int i305 = 0, i306 = 0;
                foreach (var item in jsonMcpd.grievances)
                {
                    if (item.planCode == "305")
                    {
                        item.cin = GlobalViewModel.TestCin305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        i306++;
                        if (i306 >= 10) i306 = 0;
                    }
                }
                i305 = 0;
                i306 = 0;
                foreach (var item in jsonMcpd.appeals)
                {
                    if (item.planCode == "305")
                    {
                        item.cin = GlobalViewModel.TestCin305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        i306++;
                        if (i306 >= 10) i306 = 0;
                    }
                }
                i305 = 0;
                i306 = 0;
                foreach (var item in jsonMcpd.continuityOfCare)
                {
                    if (item.planCode == "305")
                    {
                        item.cin = GlobalViewModel.TestCin305[i305];
                        if (item.cocType == "MER Denial") item.merExemptionId = GlobalViewModel.TestCocMer305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        if (item.cocType == "MER Denial") item.merExemptionId = GlobalViewModel.TestCocMer306[i306];
                        i306++;
                        if (i306 >= 10) i306 = 0;
                    }
                }
                i305 = 0;
                i306 = 0;
                foreach (var item in jsonMcpd.outOfNetwork)
                {
                    if (item.planCode == "305")
                    {
                        item.cin = GlobalViewModel.TestCin305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        i306++;
                        if (i306 >= 10) i306 = 0;
                    }
                }
                string fileName = "IEHP_MCPD_" + model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd") + "_" + model.FileVersion + ".json";
                System.IO.File.WriteAllText(Path.Combine(model.JsonExportPath, fileName), JsonOperations.GetMcpdJson(jsonMcpd));
                model.mcpdJsonMessage = $"Test Json file {fileName} generation in {model.JsonExportPath} completed";
            }
            return View(model);

        }

        // GET: Json
        public async Task<IActionResult> PCPAjson()
        {

            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            PcpaViewModel model = new PcpaViewModel();

            model.JsonExportPath = configuration["JsonExportPath"];
            model.PcpaHeader = await _context.PcpHeaders.FirstOrDefaultAsync();
            model.PcpaHeader.SubmissionDate = DateTime.Today.ToString("yyyyMMdd");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PCPAJson(PcpaViewModel model)
        {
            model.PcpaHeader = await _context.PcpHeaders.FirstOrDefaultAsync();
            model.PcpaHeader.SubmissionDate = DateTime.Today.ToString("yyyyMMdd");
            if (model.SelectedJsonFileMode == "Production")
            {
                //check processlog, if someone else is working on the same task
                ProcessLog processLog = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.RunStatus == "1");
                if (processLog != null)
                {
                    model.PcpaMessage = $"Same task has been initialized by {processLog.RunBy }";
                    return View(model);
                }
                else
                {
                    foreach (string IPA in model.Ipas.Except(new List<string>() { "All" }))
                    {
                        processLog = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.TradingPartnerCode == IPA && x.RecordYear == model.PcpaHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.PcpaHeader.ReportingPeriod.Substring(4, 2));
                        if (processLog == null)
                        {
                            processLog = new ProcessLog
                            {
                                TradingPartnerCode = IPA,
                                RecordYear = model.PcpaHeader.ReportingPeriod.Substring(0, 4),
                                RecordMonth = model.PcpaHeader.ReportingPeriod.Substring(4, 2),
                                RunStatus = "1",
                                RunTime = DateTime.Now,
                                RunBy = User.Identity.Name
                            };
                            _contextLog.Add(processLog);
                        }
                        else
                        {
                            processLog.RunStatus = "1";
                            processLog.RunTime = DateTime.Now;
                            processLog.RunBy = User.Identity.Name;
                        }
                        await _contextLog.SaveChangesAsync();
                    }
                }
                PcpHeader headerHistory = await _contextHistory.PcpHeaders.FirstOrDefaultAsync(x => x.ReportingPeriod.Substring(0, 6) == model.PcpaHeader.ReportingPeriod.Substring(0, 6));
                if (headerHistory != null)
                {
                    await _contextHistory.Database.ExecuteSqlCommandAsync($"delete from History.PcpAssignment where PcpHeaderId={headerHistory.PcpHeaderId.ToString()}");
                    _contextHistory.PcpHeaders.Remove(headerHistory);
                    await _contextHistory.SaveChangesAsync();
                }
                PcpHeader headerError = await _contextError.PcpHeaders.FirstOrDefaultAsync(x => x.ReportingPeriod.Substring(0, 6) == model.PcpaHeader.ReportingPeriod.Substring(0, 6));
                if (headerError != null)
                {
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.PcpAssignment where PcpHeaderId={headerError.PcpHeaderId.ToString()}");
                    _contextError.PcpHeaders.Remove(headerError);
                    await _contextError.SaveChangesAsync();
                }
                var dupCins = _context.PcpAssignments.GroupBy(x => x.Cin).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                List<PcpAssignment> validPcpas = new List<PcpAssignment>();
                List<PcpAssignment> errorPcpas = new List<PcpAssignment>();
                int totalPages = _context.PcpAssignments.Count() / 100000;
                string cinPattern = "^[0-9]{8}[A-Z]$";
                string npiPattern = "^[0-9]{10}$";
                for (int i = 0; i <= totalPages; i++)
                {
                    List<PcpAssignment> currentPcpas = await _context.PcpAssignments.Skip(i * 100000).Take(100000).ToListAsync();
                    if (currentPcpas.Count() == 0) break;
                    //List<Tuple<string, string, bool, string>> pcpaSchemas = JsonOperations.ValidatePcpa(currentPcpas);
                    foreach (PcpAssignment pcpa in currentPcpas)
                    {
                        if (dupCins.Contains(pcpa.Cin))
                        {
                            pcpa.ErrorMessage = "Business Error: Duplicated Cin";
                            errorPcpas.Add(pcpa);
                        }
                        else if (pcpa.PlanCode != "305" && pcpa.PlanCode != "306")
                        {
                            pcpa.ErrorMessage = "Business Error: Invalid PlanCode";
                            errorPcpas.Add(pcpa);
                        }
                        else if (!System.Text.RegularExpressions.Regex.Match(pcpa.Cin, cinPattern).Success)
                        {
                            pcpa.ErrorMessage = "Schema Error: Invalid CIN";
                            errorPcpas.Add(pcpa);
                        }
                        else if (!System.Text.RegularExpressions.Regex.Match(pcpa.Npi, npiPattern).Success)
                        {
                            pcpa.ErrorMessage = "Schema Error: Invalid NPI";
                            errorPcpas.Add(pcpa);
                        }
                        else
                        {
                            validPcpas.Add(pcpa);
                        }
                    }
                }
                headerHistory = new PcpHeader
                {
                    PlanParent = model.PcpaHeader.PlanParent,
                    ReportingPeriod = model.PcpaHeader.ReportingPeriod,
                    SubmissionDate = model.PcpaHeader.SubmissionDate,
                    SubmissionType = model.PcpaHeader.SubmissionType,
                    SubmissionVersion = model.PcpaHeader.SubmissionVersion,
                    SchemaVersion = model.PcpaHeader.SchemaVersion
                };
                _contextHistory.PcpHeaders.Add(headerHistory);
                await _contextHistory.SaveChangesAsync();
                for (int j = 0; j <= validPcpas.Count() / 100000; j++)
                {
                    _contextHistory.PcpAssignments.AddRange(validPcpas.Skip(j * 100000).Take(100000).Select(x => new PcpAssignment
                    {
                        PcpHeaderId = headerHistory.PcpHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        Npi = x.Npi,
                        TradingPartnerCode = x.TradingPartnerCode
                    }));
                    await _contextHistory.SaveChangesAsync();
                }
                if (errorPcpas.Count() > 0)
                {
                    headerError = new PcpHeader
                    {
                        PlanParent = model.PcpaHeader.PlanParent,
                        ReportingPeriod = model.PcpaHeader.ReportingPeriod,
                        SubmissionDate = model.PcpaHeader.SubmissionDate,
                        SubmissionType = model.PcpaHeader.SubmissionType,
                        SubmissionVersion = model.PcpaHeader.SubmissionVersion,
                        SchemaVersion = model.PcpaHeader.SchemaVersion
                    };
                    _contextError.PcpHeaders.Add(headerError);
                    await _contextError.SaveChangesAsync();
                    _contextError.PcpAssignments.AddRange(errorPcpas.Select(x => new PcpAssignment
                    {
                        PcpHeaderId = headerError.PcpHeaderId,
                        PlanCode = x.PlanCode,
                        Cin = x.Cin,
                        Npi = x.Npi,
                        TradingPartnerCode = x.TradingPartnerCode,
                        ErrorMessage = x.ErrorMessage
                    }));
                    await _contextError.SaveChangesAsync();
                }

                JsonPcpa jsonPcpa = new JsonPcpa();
                jsonPcpa.header = new JsonPcpaHeader
                {
                    planParent = model.PcpaHeader.PlanParent,
                    reportingPeriod = model.PcpaHeader.ReportingPeriod,
                    submissionDate = model.PcpaHeader.SubmissionDate,
                    submissionType = model.PcpaHeader.SubmissionType,
                    submissionVersion = model.PcpaHeader.SubmissionVersion,
                    schemaVersion = model.PcpaHeader.SchemaVersion
                };
                jsonPcpa.pcpa = validPcpas.Select(x => new JsonPcpaDetail
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    npi = x.Npi
                }).ToList();
                string fileName = "IEHP_PCPA_" + model.PcpaHeader.SubmissionDate + "_" + model.FileVersion + ".json";
                System.IO.File.WriteAllText(Path.Combine(model.JsonExportPath, fileName), JsonOperations.GetPcpaJson(jsonPcpa));
                model.PcpaMessage = $"Json file {fileName} generation in {model.JsonExportPath} completed";
                SubmissionLog log2 = await _contextLog.SubmissionLogs.FirstOrDefaultAsync(x => x.RecordYear == model.PcpaHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.PcpaHeader.ReportingPeriod.Substring(4, 2) && x.FileType == "PCPA");
                if (log2 == null)
                {
                    log2 = new SubmissionLog
                    {
                        RecordYear = model.PcpaHeader.ReportingPeriod.Substring(0, 4),
                        RecordMonth = model.PcpaHeader.ReportingPeriod.Substring(4, 2),
                        FileName = fileName,
                        FileType = "PCPA",
                        SubmitterName = "IEHP",
                        SubmissionDate = model.PcpaHeader.SubmissionDate,
                        CreateDate = DateTime.Now,
                        CreateBy = User.Identity.Name
                    };
                    _contextLog.Add(log2);
                    await _contextLog.SaveChangesAsync();
                }
                log2.FileName = fileName;
                log2.SubmissionDate = model.PcpaHeader.SubmissionDate;
                log2.TotalPCPASubmitted = validPcpas.Count();
                log2.UpdateDate = DateTime.Now;
                log2.UpdateBy = User.Identity.Name;
                await _contextLog.SaveChangesAsync();
                foreach (string IPA in model.Ipas.Except(new List<string>() { "All" }))
                {
                    ProcessLog log = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.TradingPartnerCode == IPA && x.RecordYear == model.PcpaHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.PcpaHeader.ReportingPeriod.Substring(4, 2));
                    int countValid = validPcpas.Count(x => x.TradingPartnerCode == IPA);
                    int countError = errorPcpas.Count(x => x.TradingPartnerCode == IPA);
                    log.PCPAErrors = countError;
                    log.PCPASubmits = countValid;
                    log.PCPATotal = countValid + countError;
                    log.RunStatus = "0";
                    await _contextLog.SaveChangesAsync();
                }
            }
            else
            {
                JsonPcpa jsonPcpa = new JsonPcpa();
                jsonPcpa.header = new JsonPcpaHeader
                {
                    planParent = model.PcpaHeader.PlanParent,
                    reportingPeriod = model.PcpaHeader.ReportingPeriod,
                    submissionDate = model.PcpaHeader.SubmissionDate,
                    submissionType = model.PcpaHeader.SubmissionType,
                    submissionVersion = model.PcpaHeader.SubmissionVersion,
                    schemaVersion = model.PcpaHeader.SchemaVersion
                };
                jsonPcpa.pcpa = _context.PcpAssignments.Select(x => new JsonPcpaDetail
                {
                    planCode = x.PlanCode,
                    cin = x.Cin,
                    npi = x.Npi
                }).ToList();
                int i305 = 0, i306 = 0;
                foreach (var item in jsonPcpa.pcpa)
                {
                    if (item.planCode == "305")
                    {
                        item.cin = GlobalViewModel.TestCin305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        i306++;
                        if (i306 >= 10) i306 = 0;
                    }
                }
                string fileName = "IEHP_PCPA_" + model.PcpaHeader.SubmissionDate + "_" + model.FileVersion + ".json";
                System.IO.File.WriteAllText(Path.Combine(model.JsonExportPath, fileName), JsonOperations.GetPcpaJson(jsonPcpa));
                model.PcpaMessage = $"Test Json file {fileName} generation in {model.JsonExportPath} completed";
            }
            return View(model);

        }
        public IActionResult PcpaFileValidation()
        {
            JsonFileViewModel model = new JsonFileViewModel();
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            model.FilePath = configuration["JsonExportPath"];
            model.SelectedFileType = "PCPA";
            List<string> listedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).Where(x => x.ToUpper().Contains("PCPA")).ToList();
            HttpContext.Session.Set("PcpaFiles", listedFiles);
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            foreach (var s in listedFiles)
            {
                model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
            }
            return View(model);
        }
        [HttpPost]
        public IActionResult PcpaFileValidation(JsonFileViewModel model)
        {
            string[] selects = model.SelectedItems.Split(',');
            List<string> listedFiles = HttpContext.Session.Get<List<string>>("PcpaFiles");
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            if (listedFiles == null)
            {
                return View(model);
            }
            int ii = 0;
            foreach (var s in listedFiles)
            {
                if (selects[ii] == "1")
                {
                    string jsonFile = System.IO.File.ReadAllText(Path.Combine(model.FilePath, s));
                    Tuple<bool, string> result = JsonOperations.ValidatePcpaFile(ref jsonFile);
                    model.SelectedFiles.Add(Tuple.Create(true, s, result.Item1 ? "Passed" : "Failed", "FileId" + ii.ToString(), result.Item2));
                }
                else
                {
                    model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
                }
                ii++;
            }
            return View(model);
        }
        public IActionResult PcpaBrowseFile(JsonFileViewModel model)
        {
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            if (Directory.Exists(model.FilePath))
            {
                List<string> listedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).Where(x => x.ToUpper().Contains("PCPA")).ToList();
                HttpContext.Session.Set("PcpaFiles", listedFiles);
                foreach (var s in listedFiles)
                {
                    model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
                }

            }

            return View(model);
        }
        public IActionResult McpdFileValidation()
        {
            JsonFileViewModel model = new JsonFileViewModel();
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            model.FilePath = configuration["JsonExportPath"];
            model.SelectedFileType = "MCPD";
            List<string> listedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).Where(x => x.ToUpper().Contains("MCPD")).ToList();
            HttpContext.Session.Set("McpdFiles", listedFiles);
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            foreach (var s in listedFiles)
            {
                model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult McpdFileValidation(JsonFileViewModel model)
        {
            string[] selects = model.SelectedItems.Split(',');
            List<string> listedFiles = HttpContext.Session.Get<List<string>>("McpdFiles");
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            if (listedFiles == null)
            {
                return View(model);
            }
            int ii = 0;
            foreach (var s in listedFiles)
            {
                if (selects[ii] == "1")
                {
                    string jsonFile = System.IO.File.ReadAllText(Path.Combine(model.FilePath, s));
                    Tuple<bool, string> result = JsonOperations.ValidateMcpdFile(ref jsonFile);
                    model.SelectedFiles.Add(Tuple.Create(true, s, result.Item1 ? "Passed" : "Failed", "FileId" + ii.ToString(), result.Item2));
                }
                else
                {
                    model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
                }
                ii++;
            }
            return View(model);
        }
        public IActionResult McpdBrowseFile(JsonFileViewModel model)
        {
            model.SelectedFiles = new List<Tuple<bool, string, string, string, string>>();
            if (Directory.Exists(model.FilePath))
            {
                List<string> listedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).Where(x => x.ToUpper().Contains("MCPD")).ToList();
                HttpContext.Session.Set("McpdFiles", listedFiles);
                foreach (var s in listedFiles)
                {
                    model.SelectedFiles.Add(Tuple.Create(false, s, "", "", ""));
                }

            }
            return View(model);
        }
        [HttpPost]
        public IActionResult McpdJsonTest(int? id, McpdViewModel model)
        {
            //this is for test only
            return View(model);
        }
    }
}

