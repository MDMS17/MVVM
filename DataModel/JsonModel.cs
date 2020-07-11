using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcpdipData
{
    public class JsonPcpaHeader
    {
        public string planParent { get; set; }
        public string submissionDate { get; set; }
        public string reportingPeriod { get; set; }
        public string submissionType { get; set; }
        public string submissionVersion { get; set; }
        public string schemaVersion { get; set; }
    }
    public class JsonPcpaDetail
    {
        public string planCode { get; set; }
        public string cin { get; set; }
        public string npi { get; set; }
    }
    public class JsonPcpa
    {
        public JsonPcpaHeader header { get; set; }
        public List<JsonPcpaDetail> pcpa { get; set; }
    }
    public class JsonGrievance
    {
        public string planCode { get; set; }
        public string cin { get; set; }
        public string grievanceId { get; set; }
        public string recordType { get; set; }
        public string parentGrievanceId { get; set; }
        public string grievanceReceivedDate { get; set; }
        public List<string> grievanceType { get; set; }
        public string benefitType { get; set; }
        public string exemptIndicator { get; set; }
    }
    public class JsonAppeal
    {
        public string planCode { get; set; }
        public string cin { get; set; }
        public string appealId { get; set; }
        public string recordType { get; set; }
        public string parentGrievanceId { get; set; }
        public string parentAppealId { get; set; }
        public string appealReceivedDate { get; set; }
        public string noticeOfActionDate { get; set; }
        public string appealType { get; set; }
        public string benefitType { get; set; }
        public string appealResolutionStatusIndicator { get; set; }
        public string appealResolutionDate { get; set; }
        public string partiallyOverturnIndicator { get; set; }
        public string expeditedIndicator { get; set; }
    }
    public class JsonCOC
    {
        public string planCode { get; set; }
        public string cin { get; set; }
        public string cocId { get; set; }
        public string recordType { get; set; }
        public string parentCocId { get; set; }
        public string cocReceivedDate { get; set; }
        public string cocType { get; set; }
        public string benefitType { get; set; }
        public string cocDispositionIndicator { get; set; }
        public string cocExpirationDate { get; set; }
        public string cocDenialReasonIndicator { get; set; }
        public string submittingProviderNpi { get; set; }
        public string cocProviderNpi { get; set; }
        public string providerTaxonomy { get; set; }
        public string merExemptionId { get; set; }
        public string exemptionToEnrollmentDenialCode { get; set; }
        public string exemptionToEnrollmentDenialDate { get; set; }
        public string merCocDispositionIndicator { get; set; }
        public string merCocDispositionDate { get; set; }
        public string reasonMerCocNotMetIndicator { get; set; }
    }
    public class JsonOON
    {
        public string planCode { get; set; }
        public string cin { get; set; }
        public string oonId { get; set; }
        public string recordType { get; set; }
        public string parentOonId { get; set; }
        public string oonRequestReceivedDate { get; set; }
        public string referralRequestReasonIndicator { get; set; }
        public string oonResolutionStatusIndicator { get; set; }
        public string oonRequestResolvedDate { get; set; }
        public string partialApprovalExplanation { get; set; }
        public string specialistProviderNpi { get; set; }
        public string providerTaxonomy { get; set; }
        public string serviceLocationAddressLine1 { get; set; }
        public string serviceLocationAddressLine2 { get; set; }
        public string serviceLocationCity { get; set; }
        public string serviceLocationState { get; set; }
        public string serviceLocationZip { get; set; }
        public string serviceLocationCountry { get; set; }
    }
    public class JsonMcpdHeader
    {
        public string planParent { get; set; }
        public string submissionDate { get; set; }
        public string schemaVersion { get; set; }
    }
    public class JsonMcpd
    {
        public JsonMcpdHeader header { get; set; }
        public List<JsonGrievance> grievances { get; set; }
        public List<JsonAppeal> appeals { get; set; }
        public List<JsonCOC> continuityOfCare { get; set; }
        public List<JsonOON> outOfNetwork { get; set; }
    }
    public class TestJsonGrievance
    {
        public string mcpdGrievanceId { get; set; }
        public string mcpdHeaderId { get; set; }
        public string planCode { get; set; }
        public string cin { get; set; }
        public string grievanceId { get; set; }
        public string recordType { get; set; }
        public string parentGrievanceId { get; set; }
        public string grievanceReceivedDate { get; set; }
        public string grievanceType { get; set; }
        public string benefitType { get; set; }
        public string exemptIndicator { get; set; }
        public string tradingPartnerCode { get; set; }
        public string errorMessage { get; set; }
        public string dataSource { get; set; }
    }
    public class TestJsonAppeal
    {
        public string mcpdAppealId { get; set; }
        public string mcpdHeaderId { get; set; }
        public string planCode { get; set; }
        public string cin { get; set; }
        public string appealId { get; set; }
        public string recordType { get; set; }
        public string parentGrievanceId { get; set; }
        public string parentAppealId { get; set; }
        public string appealReceivedDate { get; set; }
        public string noticeOfActionDate { get; set; }
        public string appealType { get; set; }
        public string benefitType { get; set; }
        public string appealResolutionStatusIndicator { get; set; }
        public string appealResolutionDate { get; set; }
        public string partiallyOverturnIndicator { get; set; }
        public string expeditedIndicator { get; set; }
        public string tradingPartnerCode { get; set; }
        public string errorMessage { get; set; }
        public string dataSource { get; set; }
    }
    public class TestJsonPcpa
    {
        public string pcpAssignmentId { get; set; }
        public string pcpHeaderId { get; set; }
        public string planCode { get; set; }
        public string cin { get; set; }
        public string npi { get; set; }
        public string tradingPartnerCode { get; set; }
        public string errorMessage { get; set; }
        public string dataSource { get; set; }
    }
}

