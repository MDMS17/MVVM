using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mcpdipData;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonLib
{
    public static class JsonOperations
    {
        public static string GetTestJson(ref ResponseFile responseFile)
        {
            return JsonConvert.SerializeObject(responseFile, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        }
        public static string GetMcpdJson(JsonMcpd jsonMcpd)
        {
            return JsonConvert.SerializeObject(
                jsonMcpd,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetPcpaJson(JsonPcpa jsonPcpa)
        {
            return JsonConvert.SerializeObject(
                jsonPcpa,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetPcpaHeaderJson(PcpHeader pcpHeader)
        {
            return JsonConvert.SerializeObject(
                pcpHeader,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }

        public static bool ValidatePcpa(string pcpJsonString, out IList<string> messages)
        {
            var PcpaSchemaFile = System.IO.File.ReadAllText("JsonSchema\\pcpa.json");

            var schema = JSchema.Parse(PcpaSchemaFile);

            JObject PcpJson = JObject.Parse(pcpJsonString);

            var isValid = PcpJson.IsValid(schema, out messages);

            return isValid;
        }
        public static string GetPcpaDetailJson(List<PcpAssignment> pcpAssignments)
        {
            return JsonConvert.SerializeObject(
                pcpAssignments,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetMcpdHeaderJson(McpdHeader mcpdHeader)
        {
            return JsonConvert.SerializeObject(
                mcpdHeader,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetGrievanceJson(List<McpdGrievance> grievances)
        {
            return JsonConvert.SerializeObject(
                grievances,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetAppealJson(List<McpdAppeal> appeals)
        {
            return JsonConvert.SerializeObject(
                appeals,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetCocJson(List<McpdContinuityOfCare> Cocs)
        {
            return JsonConvert.SerializeObject(
                Cocs,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static string GetOonJson(List<McpdOutOfNetwork> Oons)
        {
            return JsonConvert.SerializeObject(
                Oons,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
        }
        public static bool ValidateMcpd(string jsonString, out IList<string> messages)
        {
            var McpdSchemaFile = System.IO.File.ReadAllText("JsonSchema\\mcpd.json");

            var schema = JSchema.Parse(McpdSchemaFile);

            JObject McpdJson = JObject.Parse(jsonString);

            var isValid = McpdJson.IsValid(schema, out messages);

            return isValid;
        }
        public static List<Tuple<string, bool, string>> ValidateGrievance(List<McpdGrievance> grievances)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            List<JsonGrievance> jsonGrievances = grievances.Select(x => new JsonGrievance
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

            var GrievanceSchemaFile = System.IO.File.ReadAllText("JsonSchema\\grievance.json");
            var schema = JSchema.Parse(GrievanceSchemaFile);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonGrievance grievance in jsonGrievances)
            {
                JObject GrievanceObject = JObject.Parse(JsonConvert.SerializeObject(grievance, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                IList<string> errors = new List<string>();
                bool isValid = GrievanceObject.IsValid(schema, out errors);
                result.Add(Tuple.Create(grievance.grievanceId, isValid, String.Join("~", errors)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateAppeal(List<McpdAppeal> appeals)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            List<JsonAppeal> jsonAppeals = appeals.Select(x => new JsonAppeal
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
            var AppealSchemaFile = System.IO.File.ReadAllText("JsonSchema\\appeal.json");
            var schema = JSchema.Parse(AppealSchemaFile);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonAppeal appeal in jsonAppeals)
            {
                JObject AppealObject = JObject.Parse(JsonConvert.SerializeObject(appeal, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                IList<string> errors = new List<string>();
                bool isValid = AppealObject.IsValid(schema, out errors);
                result.Add(Tuple.Create(appeal.appealId, isValid, String.Join("~", errors)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateCOC(List<McpdContinuityOfCare> cocs)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            List<JsonCOC> jsonCOCs = cocs.Select(x => new JsonCOC
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
            var COCSchemaFile = System.IO.File.ReadAllText("JsonSchema\\coc.json");
            var schema = JSchema.Parse(COCSchemaFile);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonCOC coc in jsonCOCs)
            {
                JObject COCObject = JObject.Parse(JsonConvert.SerializeObject(coc, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                IList<string> errors = new List<string>();
                bool isValid = COCObject.IsValid(schema, out errors);
                result.Add(Tuple.Create(coc.cocId, isValid, String.Join("~", errors)));
            }
            return result;
        }
        public static List<Tuple<string, bool, string>> ValidateOON(List<McpdOutOfNetwork> oons)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            List<JsonOON> jsonOONs = oons.Select(x => new JsonOON
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
            var OONSchemaFile = System.IO.File.ReadAllText("JsonSchema\\oon.json");
            var schema = JSchema.Parse(OONSchemaFile);
            List<Tuple<string, bool, string>> result = new List<Tuple<string, bool, string>>();
            foreach (JsonOON oon in jsonOONs)
            {
                JObject OONObject = JObject.Parse(JsonConvert.SerializeObject(oon, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                IList<string> errors = new List<string>();
                bool isValid = OONObject.IsValid(schema, out errors);
                result.Add(Tuple.Create(oon.oonId, isValid, String.Join("~", errors)));
            }
            return result;
        }
        public static List<Tuple<string, string, bool, string>> ValidatePcpa(List<PcpAssignment> Pcpas)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            List<JsonPcpaDetail> jsonPcpas = Pcpas.Select(x => new JsonPcpaDetail
            {
                planCode = x.PlanCode,
                cin = x.Cin,
                npi = x.Npi
            }).ToList();
            var PcpaSchemaFile = System.IO.File.ReadAllText("JsonSchema\\pcpaitem.json");
            var schema = JSchema.Parse(PcpaSchemaFile);
            List<Tuple<string, string, bool, string>> result = new List<Tuple<string, string, bool, string>>();
            foreach (JsonPcpaDetail pcpa in jsonPcpas)
            {
                JObject PcpaObject = JObject.Parse(JsonConvert.SerializeObject(pcpa, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                IList<string> errors = new List<string>();
                bool isValid = PcpaObject.IsValid(schema, out errors);
                result.Add(Tuple.Create(pcpa.cin, pcpa.npi, isValid, String.Join("~", errors)));
            }
            return result;
        }
        public static Tuple<bool, string> ValidateMcpdFile(ref string jsonString)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            var McpdSchemaFile = System.IO.File.ReadAllText("JsonSchema\\mcpd.json");
            var schema = JSchema.Parse(McpdSchemaFile);
            JObject jsonMcpdObject = JObject.Parse(jsonString);
            IList<string> errors = new List<string>();
            bool isValid = jsonMcpdObject.IsValid(schema, out errors);
            return Tuple.Create(isValid, String.Join("~", errors));
        }
        public static Tuple<bool, string> ValidatePcpaFile(ref string jsonString)
        {
            License.RegisterLicense("4245-raQuQdA25//Wl508WD7uddlGeTLM4eF0QVm83kpmOkdJNjFc9NyQSNSQh4zwlqBQ4M6SWxAOk/7suUX1wug3fiywJUzj3aryB29DpQQjtxHxNmjPeqrFbK6Wqo1AlEK9vaAmorQ6bCOex25x8q2CrStc2ACx2ffBCe9/qi7VS/N7IklkIjo0MjQ1LCJFeHBpcnlEYXRlIjoiMjAyMS0wNS0yMVQxNjowMjoyOC43OTEwNjMxWiIsIlR5cGUiOiJKc29uU2NoZW1hQnVzaW5lc3MifQ==");
            var PcpaSchemaFile = System.IO.File.ReadAllText("JsonSchema\\pcpa.json");
            var schema = JSchema.Parse(PcpaSchemaFile);
            JObject jsonPcpaObject = JObject.Parse(jsonString);
            IList<string> errors = new List<string>();
            bool isValid = jsonPcpaObject.IsValid(schema, out errors);
            return Tuple.Create(isValid, String.Join("~", errors));
        }
    }
}
