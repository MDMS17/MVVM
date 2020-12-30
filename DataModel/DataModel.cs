using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace mcpdipData
{
    public class McpdAppeal : HasAttributes, HasMcpdAttributes
    {
        [Key]
        public long McpdAppealId { get; set; }
        public long McpdHeaderId { get; set; }
        public string PlanCode { get; set; }
        public string Cin { get; set; }
        public string AppealId { get; set; }
        public string RecordType { get; set; }
        public string ParentGrievanceId { get; set; }
        public string ParentAppealId { get; set; }
        public string AppealReceivedDate { get; set; }
        public string NoticeOfActionDate { get; set; }
        public string AppealType { get; set; }
        public string BenefitType { get; set; }
        public string AppealResolutionStatusIndicator { get; set; }
        public string AppealResolutionDate { get; set; }
        public string PartiallyOverturnIndicator { get; set; }
        public string ExpeditedIndicator { get; set; }
        public string TradingPartnerCode { get; set; }
        public string ErrorMessage { get; set; }
        public string DataSource { get; set; }
        public string CaseNumber { get; set; }
        public string CaseStatus { get; set; }
    }

    public class McpdContinuityOfCare : HasAttributes, HasMcpdAttributes
    {
        [Key]
        public long McpdContinuityOfCareId { get; set; }
        public long McpdHeaderId { get; set; }
        public string PlanCode { get; set; }
        public string Cin { get; set; }
        public string CocId { get; set; }
        public string RecordType { get; set; }
        public string ParentCocId { get; set; }
        public string CocReceivedDate { get; set; }
        public string CocType { get; set; }
        public string BenefitType { get; set; }
        public string CocDispositionIndicator { get; set; }
        public string CocExpirationDate { get; set; }
        public string CocDenialReasonIndicator { get; set; }
        public string SubmittingProviderNpi { get; set; }
        public string CocProviderNpi { get; set; }
        public string ProviderTaxonomy { get; set; }
        public string MerExemptionId { get; set; }
        public string ExemptionToEnrollmentDenialCode { get; set; }
        public string ExemptionToEnrollmentDenialDate { get; set; }
        public string MerCocDispositionIndicator { get; set; }
        public string MerCocDispositionDate { get; set; }
        public string ReasonMerCocNotMetIndicator { get; set; }
        public string TradingPartnerCode { get; set; }
        public string ErrorMessage { get; set; }
        public string DataSource { get; set; }
        public string CaseNumber { get; set; }
        public string CaseStatus { get; set; }
    }

    public class McpdGrievance : HasAttributes, HasMcpdAttributes
    {
        [Key]
        public long McpdGrievanceId { get; set; }
        public long McpdHeaderId { get; set; }
        public string PlanCode { get; set; }
        public string Cin { get; set; }
        public string GrievanceId { get; set; }
        public string RecordType { get; set; }
        public string ParentGrievanceId { get; set; }
        public string GrievanceReceivedDate { get; set; }
        public string GrievanceType { get; set; }
        public string BenefitType { get; set; }
        public string ExemptIndicator { get; set; }
        public string TradingPartnerCode { get; set; }
        public string ErrorMessage { get; set; }
        public string DataSource { get; set; }
        public string CaseNumber { get; set; }
        public string CaseStatus { get; set; }
    }

    public class McpdHeader
    {
        [Key]
        public long McpdHeaderId { get; set; }
        public string PlanParent { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string SchemaVersion { get; set; }
        public string ReportingPeriod { get; set; }
        public bool? GrievanceProcessing { get; set; }
        public bool? AppealProcessing { get; set; }
        public bool? CocProcessing { get; set; }
        public bool? OonProcessing { get; set; }
        public bool? JsonProcessing { get; set; }
    }

    public class McpdOutOfNetwork : HasAttributes
    {
        [Key]
        public long McpdOutOfNetworkId { get; set; }
        public long McpdHeaderId { get; set; }
        public string PlanCode { get; set; }
        public string Cin { get; set; }
        public string OonId { get; set; }
        public string RecordType { get; set; }
        public string ParentOonId { get; set; }
        public string OonRequestReceivedDate { get; set; }
        public string ReferralRequestReasonIndicator { get; set; }
        public string OonResolutionStatusIndicator { get; set; }
        public string OonRequestResolvedDate { get; set; }
        public string PartialApprovalExplanation { get; set; }
        public string SpecialistProviderNpi { get; set; }
        public string ProviderTaxonomy { get; set; }
        public string ServiceLocationAddressLine1 { get; set; }
        public string ServiceLocationAddressLine2 { get; set; }
        public string ServiceLocationCity { get; set; }
        public string ServiceLocationState { get; set; }
        public string ServiceLocationZip { get; set; }
        public string ServiceLocationCountry { get; set; }
        public string TradingPartnerCode { get; set; }
        public string ErrorMessage { get; set; }
        public string DataSource { get; set; }
        public string CaseNumber { get; set; }
        public string CaseStatus { get; set; }
    }

    public class PcpAssignment : HasAttributes
    {
        [Key]
        public long PcpAssignmentId { get; set; }
        public long PcpHeaderId { get; set; }
        public string PlanCode { get; set; }
        public string Cin { get; set; }
        public string Npi { get; set; }
        public string TradingPartnerCode { get; set; }
        public string ErrorMessage { get; set; }
        public string DataSource { get; set; }
    }

    public class PcpHeader
    {
        [Key]
        public long PcpHeaderId { get; set; }
        public string PlanParent { get; set; }
        public string SubmissionDate { get; set; }
        public string ReportingPeriod { get; set; }
        public string SubmissionType { get; set; }
        public string SubmissionVersion { get; set; }
        public string SchemaVersion { get; set; }
        public bool? IEHPProcessing { get; set; }
        public bool? JsonProcessing { get; set; }
    }
}
