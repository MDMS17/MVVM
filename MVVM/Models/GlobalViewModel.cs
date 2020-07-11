using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mcpdandpcpa.Models
{
    public class GlobalViewModel
    {
        public static List<string> DropdownYear { get; set; } = new List<string> { DateTime.Today.Year.ToString(), (DateTime.Today.Year - 1).ToString(), (DateTime.Today.Year - 2).ToString(), (DateTime.Today.Year - 3).ToString() };
        public static List<string> DropdownMonth { get; set; } = new List<string> { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        public static List<string> TradingPartners { get; set; } = new List<string> { "All", "IEHP", "Kaiser" };
        public static List<string> DropdownExport { get; set; } = new List<string> { ".csv", "json" };
        public static List<string> DropdownPlanCodes { get; set; } = new List<string> { "All", "305", "306" };
        public static List<string> GrievanceCascadeDropdown { get; set; } = new List<string> { "", "CIN", "RecordType", "GrievanceType", "BenefitType", "ExemptIndicator", "DataSource", "ReceiveDate", "GrievanceId", "ParentId" };
        public static List<string> AppealCascadeDropdown { get; set; } = new List<string> { "", "CIN", "AppealId", "RecordType", "ParentGrievanceId", "ParentId", "ReceiveDate", "ActionDate", "AppealType", "BenefitType", "StatusIndicator", "ResolutionDate", "OverturnIndicator", "ExpediteIndicator", "DataSource" };
        public static List<string> CocCascadeDropdown { get; set; } = new List<string> { "", "CIN", "CocId", "RecordType", "ParentCofcId", "ReceiveDate", "CocType", "BenefitType", "CocDispositionInd", "CocExpirationDate", "DenialInd", "SubmitterNpi", "ProviderNpi", "ProviderTaxonomy", "MerExemptionId", "ExemptionCode", "ExemptionDate", "MerDispositionInd", "MerDispositionDate", "MerCocNotMetInd", "DataSource" };
        public static List<string> OonCascadeDropdown { get; set; } = new List<string> { "", "CIN", "OonId", "RecordType", "ParentOofnId", "ReceiveDate", "ReferralInd", "StatusInd", "ResolveDate", "ApprovalExplain", "SpecialistNpi", "ProviderTaxonomy", "AddressLine1", "AddressLine2", "AddressCity", "AddressState", "AddressZip", "AddressCountry", "DataSource" };
        public static List<string> JsonFileMode { get; set; } = new List<string> { "Production", "Test" };
        public static List<string> JsonFileType { get; set; } = new List<string> { "MCPD", "PCPA" };
        public static List<string> PageSizeDropdown { get; set; } = new List<string> { "20", "50", "100" };
        public static string PageSizeCurrent { get; set; } = "20";
        public static string PageSizeHistory { get; set; } = "20";
        public static string PageSizeError { get; set; } = "20";
        public static List<string> TestCin305 { get; set; } = new List<string> { "32001378A", "32001379A", "32001380A", "32001381A", "32001382A", "32001383A", "32001384A", "32001385A", "32001386A", "32001387A" };
        public static List<string> TestCin306 { get; set; } = new List<string> { "32001388A", "32001389A", "32001390A", "32001391A", "32001392A", "32001393A", "32001394A", "32001395A", "32001396A", "32001397A" };
        public static List<string> TestCocMer305 { get; set; } = new List<string> { "292092", "292093", "292094", "292095", "292096", "292097", "292098", "292099", "292100", "292101" };
        public static List<string> TestCocMer306 { get; set; } = new List<string> { "293093", "293094", "293095", "293096", "293097", "293098", "293099", "293100", "293101", "293102" };
    }
}


