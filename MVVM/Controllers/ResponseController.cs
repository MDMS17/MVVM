using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mcpdandpcpa.Models;
using mcpdipData;
using Microsoft.Extensions.Configuration;
using System.IO;
using mcpdandpcpa.Extensions;

namespace mcpdandpcpa.Controllers
{
    public class ResponseController : Controller
    {
        private readonly ResponseContext _context;
        private readonly HistoryContext _contextHistory;
        private readonly LogContext _contextLog;
        private readonly StagingContext _contextStaging;

        public ResponseController(ResponseContext context, HistoryContext contextHistory, LogContext contextLog, StagingContext contextStaging)
        {
            _context = context;
            _contextHistory = contextHistory;
            _contextLog = contextLog;
            _contextStaging = contextStaging;
        }

        // GET: Response
        public async Task<IActionResult> Index()
        {
            ResponseViewModel model = new ResponseViewModel();
            model.ResponseHeaders = await GetResponseHeaders();
            return View(model);
        }

        // GET: Response/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders
               .FirstOrDefaultAsync(m => m.HeaderId == id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }

            return View(mcpdipHeader);
        }

        // GET: Response/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Response/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HeaderId,FileName,FileType,SubmitterName,SubmissionDate,ValidationStatus,Levels,ResponseHierarchy,SchemaVersion,RecordYear,RecordMonth")] McpdipHeader mcpdipHeader)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mcpdipHeader);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdipHeader);
        }

        // GET: Response/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders.FindAsync(id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }
            return View(mcpdipHeader);
        }

        // POST: Response/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HeaderId,FileName,FileType,SubmitterName,SubmissionDate,ValidationStatus,Levels,ResponseHierarchy,SchemaVersion,RecordYear,RecordMonth")] McpdipHeader mcpdipHeader)
        {
            if (id != mcpdipHeader.HeaderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mcpdipHeader);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!McpdipHeaderExists(mcpdipHeader.HeaderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mcpdipHeader);
        }

        // GET: Response/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mcpdipHeader = await _context.McpdipHeaders
                .FirstOrDefaultAsync(m => m.HeaderId == id);
            if (mcpdipHeader == null)
            {
                return NotFound();
            }

            return View(mcpdipHeader);
        }

        // POST: Response/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mcpdipHeader = await _context.McpdipHeaders.FindAsync(id);
            _context.McpdipHeaders.Remove(mcpdipHeader);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool McpdipHeaderExists(int id)
        {
            return _context.McpdipHeaders.Any(e => e.HeaderId == id);
        }

        public IActionResult LoadResponse()
        {
            ResponseViewModel model = new ResponseViewModel();
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            model.FilePath = configuration["ResponseFilePath"];
            model.ArchivePath = configuration["ResponseArchivePath"];
            model.SelectedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).ToList();
            HttpContext.Session.Set("ResponseFiles", model.SelectedFiles);
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> LoadResponse(ResponseViewModel model)
        {
            model.SelectedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).ToList();
            foreach (string fileName in model.SelectedFiles)
            {
                if (fileName.ToUpper().Contains("PCPA"))
                {
                    ResponseFile pcpaResponseFile = new ResponseFile();

                    pcpaResponseFile.responseHierarchy = new List<ResponseHierarchy>();
                    pcpaResponseFile.responseHierarchy.Add(new ResponseHierarchy { levelIdentifier = "File", sectionIdentifier = null, responses = new List<ResponseDetail>(), children = new List<ResponseChildren>() });

                    pcpaResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "Header", sectionIdentifier = null, responses = new List<ResponseDetail>() });
                    pcpaResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "PCPA", sectionIdentifier = null, responses = new List<ResponseDetail>() });


                    string ss2 = System.IO.File.ReadAllText(Path.Combine(model.FilePath, fileName));
                    pcpaResponseFile = JsonLib.JsonDeserialize.DeserializeReponseFile(ref ss2);
                    //check if it's test file or production file
                    SubmissionLog slog = _contextLog.SubmissionLogs.FirstOrDefault(x => x.FileName == pcpaResponseFile.fileName);
                    long pcpaPointer = 0;
                    PcpHeader header = null;
                    if (slog != null)
                    {
                        header = _contextHistory.PcpHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == slog.RecordYear + slog.RecordMonth);
                        pcpaPointer = _contextHistory.PcpAssignments.Where(x => x.PcpHeaderId == header.PcpHeaderId).Min(x => x.PcpAssignmentId);
                    }
                    McpdipHeader rHeader = new McpdipHeader
                    {
                        FileName = pcpaResponseFile.fileName,
                        FileType = pcpaResponseFile.fileType,
                        SubmitterName = pcpaResponseFile.submitterName,
                        SubmissionDate = pcpaResponseFile.submissionDate,
                        ValidationStatus = pcpaResponseFile.validationStatus,
                        Levels = pcpaResponseFile.levels,
                        SchemaVersion = pcpaResponseFile.schemaVersion
                    };
                    if (slog != null)
                    {
                        rHeader.RecordYear = slog.RecordYear;
                        rHeader.RecordMonth = slog.RecordMonth;
                    }
                    else
                    {
                        rHeader.RecordYear = _contextStaging.McpdHeaders.First().ReportingPeriod.Substring(0, 4);
                        rHeader.RecordMonth = _contextStaging.McpdHeaders.First().ReportingPeriod.Substring(4, 2);
                    }
                    _context.McpdipHeaders.Add(rHeader);
                    await _context.SaveChangesAsync();

                    McpdipHierarchy hierarchy = new McpdipHierarchy
                    {
                        HeaderId = rHeader.HeaderId,
                        LevelIdentifier = pcpaResponseFile.responseHierarchy[0].levelIdentifier,
                        SectionIdentifier = pcpaResponseFile.responseHierarchy[0].sectionIdentifier
                    };
                    _context.McpdipHierarchies.Add(hierarchy);
                    await _context.SaveChangesAsync();
                    if (pcpaResponseFile.responseHierarchy[0].responses.Count > 0)
                    {
                        foreach (var response in pcpaResponseFile.responseHierarchy[0].responses)
                        {
                            McpdipDetail detail = new McpdipDetail
                            {
                                ResponseTarget = "PcpaFile",
                                ChildrenId = hierarchy.HierarchyId,
                                ItemReference = response.itemReference,
                                Id = response.id,
                                Description = response.description,
                                Severity = response.severity
                            };
                        }
                    }
                    foreach (var child in pcpaResponseFile.responseHierarchy[0].children)
                    {
                        McpdipChildren rChild = new McpdipChildren
                        {
                            HierarchyId = hierarchy.HierarchyId,
                            LevelIdentifier = child.levelIdentifier == "Header" ? "PcpaHeader" : child.levelIdentifier,
                            SectionIDentifier = child.sectionIdentifier
                        };
                        _context.McpdipChildrens.Add(rChild);
                        await _context.SaveChangesAsync();
                        HashSet<int> LineNumbers = new HashSet<int>();
                        if (child.responses.Count > 0)
                        {
                            foreach (var response in child.responses)
                            {
                                McpdipDetail detail = new McpdipDetail
                                {
                                    ResponseTarget = rChild.LevelIdentifier == "Header" ? "PcpaHeader" : rChild.LevelIdentifier,
                                    ChildrenId = rChild.ChildrenId,
                                    ItemReference = response.itemReference,
                                    Id = response.id,
                                    Description = response.description,
                                    Severity = response.severity
                                };
                                if (rChild.LevelIdentifier != "Header" && slog != null)
                                {
                                    detail.OriginalTable = "History.PcpAssignment";
                                    int RecordNumber = -1;
                                    if (!int.TryParse(response.itemReference.Split('[', ']')[1], out RecordNumber)) RecordNumber = -1;
                                    detail.OriginalHeaderId = header.PcpHeaderId;
                                    if (RecordNumber >= 0)
                                    {
                                        detail.OriginalId = pcpaPointer + RecordNumber;
                                        PcpAssignment assignment = _contextHistory.PcpAssignments.Find(detail.OriginalId);
                                        if (assignment != null)
                                        {
                                            detail.OriginalCin = assignment.Cin;
                                            detail.OriginalItemId = assignment.Npi;
                                        }
                                        if (detail.Severity == "Error" && !LineNumbers.Contains(RecordNumber)) LineNumbers.Add(RecordNumber);
                                    }
                                }
                                _context.McpdipDetails.Add(detail);
                            }
                            await _context.SaveChangesAsync();
                            if (slog != null)
                            {
                                slog.TotalPCPARejected = LineNumbers.Count();
                                slog.TotalPCPAAccepted = slog.TotalPCPASubmitted - slog.TotalPCPARejected;
                                slog.UpdateDate = DateTime.Now;
                                slog.UpdateBy = User.Identity.Name;
                                await _contextLog.SaveChangesAsync();
                            }
                        }
                    }
                }
                else if (fileName.ToUpper().Contains("MCPD"))
                {
                    ResponseFile mcpdResponseFile = new ResponseFile();

                    mcpdResponseFile.responseHierarchy = new List<ResponseHierarchy>();
                    mcpdResponseFile.responseHierarchy.Add(new ResponseHierarchy { levelIdentifier = "File", sectionIdentifier = null, responses = new List<ResponseDetail>(), children = new List<ResponseChildren>() });

                    mcpdResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "Header", sectionIdentifier = null, responses = new List<ResponseDetail>() });
                    mcpdResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "Grievances", sectionIdentifier = null, responses = new List<ResponseDetail>() });
                    mcpdResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "Appeals", sectionIdentifier = null, responses = new List<ResponseDetail>() });
                    mcpdResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "ContinuityOfCare", sectionIdentifier = null, responses = new List<ResponseDetail>() });
                    mcpdResponseFile.responseHierarchy[0].children.Add(new ResponseChildren { levelIdentifier = "OutOfNetwork", sectionIdentifier = null, responses = new List<ResponseDetail>() });


                    string ss2 = System.IO.File.ReadAllText(Path.Combine(model.FilePath, fileName));
                    mcpdResponseFile = JsonLib.JsonDeserialize.DeserializeReponseFile(ref ss2);
                    //check if it's test file or production file
                    SubmissionLog slog = _contextLog.SubmissionLogs.FirstOrDefault(x => x.FileName == mcpdResponseFile.fileName);
                    long grievancePointer = 0, appealPointer = 0, cocPointer = 0, oonPointer = 0;
                    McpdHeader header = null;
                    if (slog != null)
                    {
                        header = _contextHistory.McpdHeaders.FirstOrDefault(x => x.ReportingPeriod.Substring(0, 6) == slog.RecordYear + slog.RecordMonth);
                        grievancePointer = _contextHistory.Grievances.Where(x => x.McpdHeaderId == header.McpdHeaderId).Min(x => x.McpdGrievanceId);
                        appealPointer = _contextHistory.Appeals.Where(x => x.McpdHeaderId == header.McpdHeaderId).Min(x => x.McpdAppealId);
                        cocPointer = _contextHistory.McpdContinuityOfCare.Where(x => x.McpdHeaderId == header.McpdHeaderId).Min(x => x.McpdContinuityOfCareId);
                        oonPointer = _contextHistory.McpdOutOfNetwork.Where(x => x.McpdHeaderId == header.McpdHeaderId).Min(x => x.McpdOutOfNetworkId);
                    }
                    McpdipHeader rHeader = new McpdipHeader
                    {
                        FileName = mcpdResponseFile.fileName,
                        FileType = mcpdResponseFile.fileType,
                        SubmitterName = mcpdResponseFile.submitterName,
                        SubmissionDate = mcpdResponseFile.submissionDate,
                        ValidationStatus = mcpdResponseFile.validationStatus,
                        Levels = mcpdResponseFile.levels,
                        SchemaVersion = mcpdResponseFile.schemaVersion
                    };
                    if (slog != null)
                    {
                        rHeader.RecordYear = slog.RecordYear;
                        rHeader.RecordMonth = slog.RecordMonth;
                    }
                    else
                    {
                        rHeader.RecordYear = _contextStaging.McpdHeaders.First().ReportingPeriod.Substring(0, 4);
                        rHeader.RecordMonth = _contextStaging.McpdHeaders.First().ReportingPeriod.Substring(4, 2);
                    }
                    _context.McpdipHeaders.Add(rHeader);
                    await _context.SaveChangesAsync();

                    McpdipHierarchy hierarchy = new McpdipHierarchy
                    {
                        HeaderId = rHeader.HeaderId,
                        LevelIdentifier = mcpdResponseFile.responseHierarchy[0].levelIdentifier,
                        SectionIdentifier = mcpdResponseFile.responseHierarchy[0].sectionIdentifier
                    };
                    _context.McpdipHierarchies.Add(hierarchy);
                    await _context.SaveChangesAsync();
                    if (mcpdResponseFile.responseHierarchy[0].responses.Count > 0)
                    {
                        foreach (var response in mcpdResponseFile.responseHierarchy[0].responses)
                        {
                            McpdipDetail detail = new McpdipDetail
                            {
                                ResponseTarget = "McpdFile",
                                ChildrenId = hierarchy.HierarchyId,
                                ItemReference = response.itemReference,
                                Id = response.id,
                                Description = response.description,
                                Severity = response.severity
                            };
                        }
                    }
                    HashSet<int> LineNumberG = new HashSet<int>();
                    HashSet<int> LineNumberA = new HashSet<int>();
                    HashSet<int> LineNumberC = new HashSet<int>();
                    HashSet<int> LineNumberO = new HashSet<int>();
                    foreach (var child in mcpdResponseFile.responseHierarchy[0].children)
                    {
                        McpdipChildren rChild = new McpdipChildren
                        {
                            HierarchyId = hierarchy.HierarchyId,
                            LevelIdentifier = child.levelIdentifier == "Header" ? "McpdHeader" : child.levelIdentifier,
                            SectionIDentifier = child.sectionIdentifier
                        };
                        _context.McpdipChildrens.Add(rChild);
                        await _context.SaveChangesAsync();
                        if (child.responses.Count > 0)
                        {
                            foreach (var response in child.responses)
                            {
                                McpdipDetail detail = new McpdipDetail
                                {
                                    ResponseTarget = rChild.LevelIdentifier == "Header" ? "McpdHeader" : rChild.LevelIdentifier,
                                    ChildrenId = rChild.ChildrenId,
                                    ItemReference = response.itemReference,
                                    Id = response.id,
                                    Description = response.description,
                                    Severity = response.severity
                                };
                                if (rChild.LevelIdentifier != "Header" && slog != null)
                                {
                                    int RecordNumber = -1;
                                    if (!int.TryParse(response.itemReference.Split('[', ']')[1], out RecordNumber)) RecordNumber = -1;
                                    detail.OriginalHeaderId = header.McpdHeaderId;
                                    switch (rChild.LevelIdentifier)
                                    {
                                        case "Grievances":
                                            detail.OriginalTable = "History.McpdGrievance";
                                            if (RecordNumber >= 0)
                                            {
                                                detail.OriginalId = grievancePointer + RecordNumber;
                                                McpdGrievance grievance = _contextHistory.Grievances.Find(detail.OriginalId);
                                                if (grievance != null)
                                                {
                                                    detail.OriginalCin = grievance.Cin;
                                                    detail.OriginalItemId = grievance.GrievanceId;
                                                }
                                                if (detail.Severity == "Error" && !LineNumberG.Contains(RecordNumber)) LineNumberG.Add(RecordNumber);
                                            }
                                            break;
                                        case "Appeals":
                                            detail.OriginalTable = "History.McpdAppeal";
                                            if (RecordNumber >= 0)
                                            {
                                                detail.OriginalId = appealPointer + RecordNumber;
                                                McpdAppeal appeal = _contextHistory.Appeals.Find(detail.OriginalId);
                                                if (appeal != null)
                                                {
                                                    detail.OriginalCin = appeal.Cin;
                                                    detail.OriginalItemId = appeal.AppealId;
                                                }
                                                if (detail.Severity == "Error" && !LineNumberA.Contains(RecordNumber)) LineNumberA.Add(RecordNumber);
                                            }
                                            break;
                                        case "ContinuityOfCare":
                                            detail.OriginalTable = "History.McpdContinuityOfCare";
                                            if (RecordNumber >= 0)
                                            {
                                                detail.OriginalId = cocPointer + RecordNumber;
                                                McpdContinuityOfCare coc = _contextHistory.McpdContinuityOfCare.Find(detail.OriginalId);
                                                if (coc != null)
                                                {
                                                    detail.OriginalCin = coc.Cin;
                                                    detail.OriginalItemId = coc.CocId;
                                                }
                                                if (detail.Severity == "Error" && !LineNumberC.Contains(RecordNumber)) LineNumberC.Add(RecordNumber);
                                            }
                                            break;
                                        case "OutOfNetwork":
                                            detail.OriginalTable = "History.McpdOutOfNetwork";
                                            if (RecordNumber >= 0)
                                            {
                                                detail.OriginalId = oonPointer + RecordNumber;
                                                McpdOutOfNetwork oon = _contextHistory.McpdOutOfNetwork.Find(detail.OriginalId);
                                                if (oon != null)
                                                {
                                                    detail.OriginalCin = oon.Cin;
                                                    detail.OriginalItemId = oon.OonId;
                                                }
                                                if (detail.Severity == "Error" && !LineNumberO.Contains(RecordNumber)) LineNumberO.Add(RecordNumber);
                                            }
                                            break;
                                    }
                                }
                                _context.McpdipDetails.Add(detail);
                            }
                            await _context.SaveChangesAsync();
                            if (slog != null)
                            {
                                slog.TotalGrievanceRejected = LineNumberG.Count();
                                slog.TotalGrievanceAccepted = slog.TotalGrievanceSubmitted - slog.TotalGrievanceRejected;
                                slog.TotalAppealRejected = LineNumberA.Count();
                                slog.TotalAppealAccepted = slog.TotalAppealSubmitted - slog.TotalAppealRejected;
                                slog.TotalCOCRejected = LineNumberC.Count();
                                slog.TotalCOCAccepted = slog.TotalCOCSubmitted - slog.TotalCOCRejected;
                                slog.TotalOONRejected = LineNumberO.Count();
                                slog.TotalOONAccepted = slog.TotalOONSubmitted - slog.TotalOONRejected;
                                slog.UpdateDate = DateTime.Now;
                                slog.UpdateBy = User.Identity.Name;
                                await _contextLog.SaveChangesAsync();
                            }
                        }
                    }
                }
                System.IO.File.Move(Path.Combine(model.FilePath, fileName), Path.Combine(model.ArchivePath, fileName));
            }
            model.Message = "Load Response files successful";
            return View(model);
        }
        public IActionResult ChangeResponseFilePath(ResponseViewModel model)
        {
            if (Directory.Exists(model.FilePath))
            {
                model.SelectedFiles = Directory.EnumerateFiles(model.FilePath).Select(x => Path.GetFileName(x)).ToList();
                HttpContext.Session.Set("ResponseFiles", model.SelectedFiles);
                model.Message = $"Change response path to {model.FilePath} successfule";
            }
            else
            {
                model.Message = $"Change response path to {model.FilePath} failed, restored default setting";
                IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                model.FilePath = configuration["ResponseFilePath"];
            }
            return View(model);
        }
        public IActionResult ChangeResponseArchivePath(ResponseViewModel model)
        {
            if (Directory.Exists(model.ArchivePath))
            {
                model.Message = $"Change response archive path to {model.ArchivePath} successful";
            }
            else
            {
                model.Message = $"Change response archive path to {model.ArchivePath} failed, restored default setting";
                IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                model.ArchivePath = configuration["ResponseArchivePath"];
            }
            return View(model);
        }
        private async Task<List<ResponseLogDisplayModel>> GetResponseHeaders()
        {
            var result = _context.McpdipHeaders.OrderByDescending(x => x.HeaderId).Select(x => new ResponseLogDisplayModel
            {
                RecordYear = x.RecordYear,
                RecordMonth = x.RecordMonth,
                FileName = x.FileName,
                FileType = x.FileType,
                SubmitterName = x.SubmitterName,
                SubmissionDate = x.SubmissionDate,
                ValidationStatus = x.ValidationStatus,
                Levels = x.Levels,
                SchemaVersion = x.SchemaVersion

            }).PrepareDisplay<ResponseLogDisplayModel>();

            return await Task.FromResult(result.ToList());
        }
        [HttpPost]
        public IActionResult LoadResponse(int? id, ResponseViewModel model)
        {
            //this is for test only
            return View(model);
        }
    }
}

