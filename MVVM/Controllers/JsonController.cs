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
                    foreach (string IPA in model.TradingPartners)
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
                    await _contextHistory.Database.ExecuteSqlCommandAsync($"delete from History.McpdGrievance where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextHistory.Database.ExecuteSqlCommandAsync($"delete from History.McpdAppeal where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextHistory.Database.ExecuteSqlCommandAsync($"delete from History.McpdContinuityOfCare where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextHistory.Database.ExecuteSqlCommandAsync($"delete from History.McpdOutOfNetwork where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    _contextHistory.McpdHeaders.Remove(headerHistory);
                    await _contextHistory.SaveChangesAsync();
                }
                McpdHeader headerError = await _contextError.McpdHeaders.FirstOrDefaultAsync(x => x.ReportingPeriod.Substring(0, 6) == model.mcpdHeader.ReportingPeriod.Substring(0, 6));
                if (headerError != null)
                {
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdGrievance where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdAppeal where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdContinuityOfCare where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    await _contextError.Database.ExecuteSqlCommandAsync($"delete from Error.McpdOutOfNetwork where McpdHeaderId={headerHistory.McpdHeaderId.ToString()}");
                    _contextError.McpdHeaders.Remove(headerError);
                    await _contextError.SaveChangesAsync();
                }
                headerHistory = new McpdHeader
                {
                    PlanParent = model.mcpdHeader.PlanParent,
                    ReportingPeriod = model.mcpdHeader.ReportingPeriod,
                    SubmissionDate = model.mcpdHeader.SubmissionDate,
                    SchemaVersion = model.mcpdHeader.SchemaVersion
                };
                _contextHistory.McpdHeaders.Add(headerHistory);
                await _contextHistory.SaveChangesAsync();
                //check schema for grievances, add error to errorcontext, add valid to historycontext
                List<McpdGrievance> allGrievances = await _context.Grievances.ToListAsync();
                List<Tuple<string, bool, string>> grievanceSchemas = JsonOperations.ValidateGrievance(allGrievances);
                List<McpdGrievance> validGrievances = new List<McpdGrievance>();
                List<McpdGrievance> errorGrievances = new List<McpdGrievance>();
                foreach (McpdGrievance grievance in allGrievances)
                {
                    if (grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item2)
                    {
                        validGrievances.Add(grievance);
                    }
                    else
                    {
                        grievance.ErrorMessage = grievanceSchemas.First(x => x.Item1 == grievance.GrievanceId).Item3;
                        errorGrievances.Add(grievance);
                    }
                }

                List<McpdAppeal> allAppeals = await _context.Appeals.ToListAsync();
                List<Tuple<string, bool, string>> appealSchemas = JsonOperations.ValidateAppeal(allAppeals);
                List<McpdAppeal> validAppeals = new List<McpdAppeal>();
                List<McpdAppeal> errorAppeals = new List<McpdAppeal>();
                foreach (McpdAppeal appeal in allAppeals)
                {
                    if (appealSchemas.First(x => x.Item1 == appeal.AppealId).Item2)
                    {
                        validAppeals.Add(appeal);
                    }
                    else
                    {
                        appeal.ErrorMessage = appealSchemas.First(x => x.Item1 == appeal.AppealId).Item3;
                        errorAppeals.Add(appeal);
                    }
                }

                List<McpdContinuityOfCare> allCocs = await _context.McpdContinuityOfCare.ToListAsync();
                List<Tuple<string, bool, string>> cocSchemas = JsonOperations.ValidateCOC(allCocs);
                List<McpdContinuityOfCare> validCocs = new List<McpdContinuityOfCare>();
                List<McpdContinuityOfCare> errorCocs = new List<McpdContinuityOfCare>();
                foreach (McpdContinuityOfCare coc in allCocs)
                {
                    if (cocSchemas.First(x => x.Item1 == coc.CocId).Item2)
                    {
                        validCocs.Add(coc);
                    }
                    else
                    {
                        coc.ErrorMessage = cocSchemas.First(x => x.Item1 == coc.CocId).Item3;
                        errorCocs.Add(coc);
                    }
                }

                List<McpdOutOfNetwork> allOons = await _context.McpdOutOfNetwork.ToListAsync();
                List<Tuple<string, bool, string>> oonSchemas = JsonOperations.ValidateOON(allOons);
                List<McpdOutOfNetwork> validOons = new List<McpdOutOfNetwork>();
                List<McpdOutOfNetwork> errorOons = new List<McpdOutOfNetwork>();
                foreach (McpdOutOfNetwork oon in allOons)
                {
                    if (oonSchemas.First(x => x.Item1 == oon.OonId).Item2)
                    {
                        validOons.Add(oon);
                    }
                    else
                    {
                        oon.ErrorMessage = oonSchemas.First(x => x.Item1 == oon.OonId).Item3;
                        errorOons.Add(oon);
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
                    TradingPartnerCode = x.TradingPartnerCode
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
                    TradingPartnerCode = x.TradingPartnerCode
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
                    TradingPartnerCode = x.TradingPartnerCode
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
                            TradingPartnerCode = x.TradingPartnerCode
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
                            TradingPartnerCode = x.TradingPartnerCode
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
                            ErrorMessage = x.ErrorMessage
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
                            ErrorMessage = x.ErrorMessage
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
                string fileName = "Mcpd_" + model.mcpdHeader.ReportingPeriod.Substring(0, 6) + ".json";
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
                foreach (string IPA in model.TradingPartners)
                {
                    ProcessLog log = await _contextLog.ProcessLogs.FirstOrDefaultAsync(x => x.TradingPartnerCode == IPA && x.RecordYear == model.mcpdHeader.ReportingPeriod.Substring(0, 4) && x.RecordMonth == model.mcpdHeader.ReportingPeriod.Substring(4, 2));
                    int countValidGrievance = validGrievances.Count(x => x.TradingPartnerCode == IPA);
                    int countErrorGrievance = errorGrievances.Count(x => x.TradingPartnerCode == IPA);
                    int countValidAppeal = validGrievances.Count(x => x.TradingPartnerCode == IPA);
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
                jsonMcpd.grievances = _context.Grievances.Where(x => x.RecordType == "Original").Select(x => new JsonGrievance
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
                jsonMcpd.appeals = _context.Appeals.Where(x => x.RecordType == "Original").Select(x => new JsonAppeal
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
                jsonMcpd.continuityOfCare = _context.McpdContinuityOfCare.Where(x => x.RecordType == "Original").Select(x => new JsonCOC
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
                jsonMcpd.outOfNetwork = _context.McpdOutOfNetwork.Where(x => x.RecordType == "Original").Select(x => new JsonOON
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
                        item.merExemptionId = GlobalViewModel.TestCocMer305[i305];
                        i305++;
                        if (i305 >= 10) i305 = 0;
                    }
                    else if (item.planCode == "306")
                    {
                        item.cin = GlobalViewModel.TestCin306[i306];
                        item.merExemptionId = GlobalViewModel.TestCocMer306[i306];
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
                string fileName = "IEHP_MCPD_" + model.mcpdHeader.SubmissionDate.ToString("yyyyMMdd") + "_00001.json";
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
                    foreach (string IPA in model.TradingPartners)
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
                List<PcpAssignment> validPcpas = new List<PcpAssignment>();
                List<PcpAssignment> errorPcpas = new List<PcpAssignment>();
                int totalPages = _context.PcpAssignments.Count() / 100000;
                for (int i = 0; i <= totalPages; i++)
                {
                    List<PcpAssignment> currentPcpas = await _context.PcpAssignments.Skip(i * 100000).Take(100000).ToListAsync();
                    if (currentPcpas.Count() == 0) break;
                    List<Tuple<string, string, bool, string>> pcpaSchemas = JsonOperations.ValidatePcpa(currentPcpas);
                    foreach (PcpAssignment pcpa in currentPcpas)
                    {
                        if (pcpaSchemas.First(x => x.Item1 == pcpa.Cin && x.Item2 == pcpa.Npi).Item3)
                        {
                            validPcpas.Add(pcpa);
                        }
                        else
                        {
                            pcpa.ErrorMessage = pcpaSchemas.First(x => x.Item1 == pcpa.Cin && x.Item2 == pcpa.Npi).Item4;
                            errorPcpas.Add(pcpa);
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
                for (int j = 0; j <= validPcpas.Count() / 1000; j++)
                {
                    _contextHistory.PcpAssignments.AddRange(validPcpas.Skip(j * 1000).Take(1000).Select(x => new PcpAssignment
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
                string fileName = "Pcpa_" + model.PcpaHeader.ReportingPeriod.Substring(0, 6) + ".json";
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
                foreach (string IPA in model.TradingPartners)
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
                string fileName = "IEHP_PCPA_" + model.PcpaHeader.SubmissionDate + "_00001.json";
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
    }
}

