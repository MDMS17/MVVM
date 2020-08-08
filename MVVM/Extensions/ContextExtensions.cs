using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mcpdipData;

namespace mcpdandpcpa.Extensions
{
    public static class ContextExtensions
    {
        public static IEnumerable<T> FilterByTradingPartner<T>(this IEnumerable<T> Records, string TradingPartnerCode) where T : HasAttributes
        {
            if (TradingPartnerCode == "All") return Records;
            else return Records.Where(x => x.TradingPartnerCode == TradingPartnerCode);
        }
        public static IEnumerable<T> FilterByPlanCode<T>(this IEnumerable<T> Records, string PlanCode) where T : HasAttributes
        {
            if (PlanCode == "All") return Records;
            else return Records.Where(x => x.PlanCode == PlanCode);
        }
        public static IEnumerable<T> FilterByCin<T>(this IEnumerable<T> Records, string Cin) where T : HasAttributes
        {
            if (string.IsNullOrEmpty(Cin)) return Records;
            else
            {
                List<string> CinList = Cin.Split(',').ToList();
                return Records.Where(x => CinList.Contains(x.Cin));
            }
        }
        public static IEnumerable<PcpAssignment> FilterByNpi(this IEnumerable<PcpAssignment> Records, string Npi)
        {
            if (string.IsNullOrEmpty(Npi)) return Records;
            else
            {
                List<string> NpiList = Npi.Split(',').ToList();
                return Records.Where(x => NpiList.Contains(x.Npi));
            }
        }
        public static IEnumerable<T> FilterByRecordType<T>(this IEnumerable<T> Records, string RecordType) where T : HasMcpdAttributes
        {
            if (string.IsNullOrEmpty(RecordType)) return Records;
            else return Records.Where(x => x.RecordType == RecordType);
        }
        public static IEnumerable<T> FilterByBenefitType<T>(this IEnumerable<T> Records, string BenefitType) where T : HasMcpdAttributes
        {
            if (string.IsNullOrEmpty(BenefitType)) return Records;
            else return Records.Where(x => x.BenefitType == BenefitType);
        }
        public static IEnumerable<T> FilterByDataSource<T>(this IEnumerable<T> Records, string DataSource) where T : HasAttributes
        {
            if (string.IsNullOrEmpty(DataSource)) return Records;
            else return Records.Where(x => x.DataSource == DataSource);
        }
        public static IEnumerable<McpdGrievance> FilterByGrievanceId(this IEnumerable<McpdGrievance> Records, string GrievanceId)
        {
            if (string.IsNullOrEmpty(GrievanceId)) return Records;
            else return Records.Where(x => x.GrievanceId == GrievanceId);
        }
        public static IEnumerable<McpdGrievance> FilterByParentId(this IEnumerable<McpdGrievance> Records, string ParentId)
        {
            if (string.IsNullOrEmpty(ParentId)) return Records;
            else return Records.Where(x => x.ParentGrievanceId == ParentId);
        }
        public static IEnumerable<McpdGrievance> FilterByReceiveDate(this IEnumerable<McpdGrievance> Records, string ReceiveDate)
        {
            if (string.IsNullOrEmpty(ReceiveDate)) return Records;
            else return Records.Where(x => string.Compare(x.GrievanceReceivedDate, ReceiveDate) >= 0);
        }
        public static IEnumerable<McpdGrievance> FilterByGrievanceType(this IEnumerable<McpdGrievance> Records, string GrievanceType)
        {
            if (string.IsNullOrEmpty(GrievanceType)) return Records;
            else return Records.Where(x => x.GrievanceType == GrievanceType);
        }
        public static IEnumerable<McpdGrievance> FilterByExemptIndicator(this IEnumerable<McpdGrievance> Records, string ExemptIndicator)
        {
            if (string.IsNullOrEmpty(ExemptIndicator)) return Records;
            else return Records.Where(x => x.ExemptIndicator == ExemptIndicator);
        }
        public static IEnumerable<McpdAppeal> FilterByAppealId(this IEnumerable<McpdAppeal> Records, string AppealId)
        {
            if (string.IsNullOrEmpty(AppealId)) return Records;
            else return Records.Where(x => x.AppealId == AppealId);
        }
        public static IEnumerable<McpdAppeal> FilterByParentGrievanceId(this IEnumerable<McpdAppeal> Records, string ParentGrievanceId)
        {
            if (string.IsNullOrEmpty(ParentGrievanceId)) return Records;
            else return Records.Where(x => x.ParentGrievanceId == ParentGrievanceId);
        }
        public static IEnumerable<McpdAppeal> FilterByParentAppealId(this IEnumerable<McpdAppeal> Records, string ParentAppealId)
        {
            if (string.IsNullOrEmpty(ParentAppealId)) return Records;
            else return Records.Where(x => x.ParentAppealId == ParentAppealId);
        }
        public static IEnumerable<McpdAppeal> FilterByAppealReceiveDate(this IEnumerable<McpdAppeal> Records, string AppealReceiveDate)
        {
            if (string.IsNullOrEmpty(AppealReceiveDate)) return Records;
            else return Records.Where(x => string.Compare(x.AppealReceivedDate, AppealReceiveDate) >= 0);
        }
        public static IEnumerable<McpdAppeal> FilterByActionDate(this IEnumerable<McpdAppeal> Records, string ActionDate)
        {
            if (string.IsNullOrEmpty(ActionDate)) return Records;
            else return Records.Where(x => string.Compare(x.NoticeOfActionDate, ActionDate) >= 0);
        }
        public static IEnumerable<McpdAppeal> FilterByAppealType(this IEnumerable<McpdAppeal> Records, string AppealType)
        {
            if (string.IsNullOrEmpty(AppealType)) return Records;
            else return Records.Where(x => x.AppealType == AppealType);
        }
        public static IEnumerable<McpdAppeal> FilterByStatusIndicator(this IEnumerable<McpdAppeal> Records, string StatusIndicator)
        {
            if (string.IsNullOrEmpty(StatusIndicator)) return Records;
            else return Records.Where(x => x.AppealResolutionStatusIndicator == StatusIndicator);
        }
        public static IEnumerable<McpdAppeal> FilterByResolutionDate(this IEnumerable<McpdAppeal> Records, string ResolutionDate)
        {
            if (string.IsNullOrEmpty(ResolutionDate)) return Records;
            else return Records.Where(x => string.Compare(x.AppealResolutionDate, ResolutionDate) >= 0);
        }
        public static IEnumerable<McpdAppeal> FilterByOverturnIndicator(this IEnumerable<McpdAppeal> Records, string OverTurnIndicator)
        {
            if (string.IsNullOrEmpty(OverTurnIndicator)) return Records;
            else return Records.Where(x => x.PartiallyOverturnIndicator == OverTurnIndicator);
        }
        public static IEnumerable<McpdAppeal> FilterByExpediteIndicator(this IEnumerable<McpdAppeal> Records, string ExpediteIndicator)
        {
            if (string.IsNullOrEmpty(ExpediteIndicator)) return Records;
            else return Records.Where(x => x.ExpeditedIndicator == ExpediteIndicator);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocId(this IEnumerable<McpdContinuityOfCare> Records, string CocId)
        {
            if (string.IsNullOrEmpty(CocId)) return Records;
            else return Records.Where(x => x.CocId == CocId);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByParentCocId(this IEnumerable<McpdContinuityOfCare> Records, string ParentCocId)
        {
            if (string.IsNullOrEmpty(ParentCocId)) return Records;
            else return Records.Where(x => x.ParentCocId == ParentCocId);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocReceiveDate(this IEnumerable<McpdContinuityOfCare> Records, string CocReceiveDate)
        {
            if (string.IsNullOrEmpty(CocReceiveDate)) return Records;
            else return Records.Where(x => string.Compare(x.CocReceivedDate, CocReceiveDate) >= 0);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocType(this IEnumerable<McpdContinuityOfCare> Records, string CocType)
        {
            if (string.IsNullOrEmpty(CocType)) return Records;
            else return Records.Where(x => x.CocType == CocType);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocDispositionIndicator(this IEnumerable<McpdContinuityOfCare> Records, string DispositionInd)
        {
            if (string.IsNullOrEmpty(DispositionInd)) return Records;
            else return Records.Where(x => x.CocDispositionIndicator == DispositionInd);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocExpirationDate(this IEnumerable<McpdContinuityOfCare> Records, string CocExpirationDate)
        {
            if (string.IsNullOrEmpty(CocExpirationDate)) return Records;
            else return Records.Where(x => string.Compare(x.CocExpirationDate, CocExpirationDate) >= 0);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocDenialReasonIndicator(this IEnumerable<McpdContinuityOfCare> Records, string DenialInd)
        {
            if (string.IsNullOrEmpty(DenialInd)) return Records;
            else return Records.Where(x => x.CocDenialReasonIndicator == DenialInd);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterBySubmittingProviderNpi(this IEnumerable<McpdContinuityOfCare> Records, string SubmitterNpi)
        {
            if (string.IsNullOrEmpty(SubmitterNpi)) return Records;
            else return Records.Where(x => x.SubmittingProviderNpi == SubmitterNpi);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByCocProviderNpi(this IEnumerable<McpdContinuityOfCare> Records, string ProviderNpi)
        {
            if (string.IsNullOrEmpty(ProviderNpi)) return Records;
            else return Records.Where(x => x.CocProviderNpi == ProviderNpi);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByProviderTaxonomy(this IEnumerable<McpdContinuityOfCare> Records, string ProviderTaxonomy)
        {
            if (string.IsNullOrEmpty(ProviderTaxonomy)) return Records;
            else return Records.Where(x => x.ProviderTaxonomy == ProviderTaxonomy);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByMerExemptionId(this IEnumerable<McpdContinuityOfCare> Records, string MerExemptionId)
        {
            if (string.IsNullOrEmpty(MerExemptionId)) return Records;
            else return Records.Where(x => x.MerExemptionId == MerExemptionId);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByExemtionCode(this IEnumerable<McpdContinuityOfCare> Records, string ExemptionCode)
        {
            if (string.IsNullOrEmpty(ExemptionCode)) return Records;
            else return Records.Where(x => x.ExemptionToEnrollmentDenialCode == ExemptionCode);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByExemptionDate(this IEnumerable<McpdContinuityOfCare> Records, string ExemptionDate)
        {
            if (string.IsNullOrEmpty(ExemptionDate)) return Records;
            else return Records.Where(x => string.Compare(x.ExemptionToEnrollmentDenialDate, ExemptionDate) >= 0);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByMerDispositionInd(this IEnumerable<McpdContinuityOfCare> Records, string MerDispositionInd)
        {
            if (string.IsNullOrEmpty(MerDispositionInd)) return Records;
            else return Records.Where(x => x.MerCocDispositionIndicator == MerDispositionInd);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByMerDispositionDate(this IEnumerable<McpdContinuityOfCare> Records, string MerDispositionDate)
        {
            if (string.IsNullOrEmpty(MerDispositionDate)) return Records;
            else return Records.Where(x => string.Compare(x.MerCocDispositionDate, MerDispositionDate) >= 0);
        }
        public static IEnumerable<McpdContinuityOfCare> FilterByMerCocNotMetInd(this IEnumerable<McpdContinuityOfCare> Records, string MerCocNotMetInd)
        {
            if (string.IsNullOrEmpty(MerCocNotMetInd)) return Records;
            else return Records.Where(x => x.ReasonMerCocNotMetIndicator == MerCocNotMetInd);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByOonId(this IEnumerable<McpdOutOfNetwork> Records, string OonId)
        {
            if (string.IsNullOrEmpty(OonId)) return Records;
            else return Records.Where(x => x.OonId == OonId);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByOonRecordType(this IEnumerable<McpdOutOfNetwork> Records, string RecordType)
        {
            if (string.IsNullOrEmpty(RecordType)) return Records;
            else return Records.Where(x => x.RecordType == RecordType);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByParentOonId(this IEnumerable<McpdOutOfNetwork> Records, string ParentOonId)
        {
            if (string.IsNullOrEmpty(ParentOonId)) return Records;
            else return Records.Where(x => x.ParentOonId == ParentOonId);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByOonReceiveDate(this IEnumerable<McpdOutOfNetwork> Records, string ReceiveDate)
        {
            if (string.IsNullOrEmpty(ReceiveDate)) return Records;
            else return Records.Where(x => string.Compare(x.OonRequestReceivedDate, ReceiveDate) >= 0);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByReferralInd(this IEnumerable<McpdOutOfNetwork> Records, string ReferralInd)
        {
            if (string.IsNullOrEmpty(ReferralInd)) return Records;
            else return Records.Where(x => x.ReferralRequestReasonIndicator == ReferralInd);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByOonStatusInd(this IEnumerable<McpdOutOfNetwork> Records, string StatusInd)
        {
            if (string.IsNullOrEmpty(StatusInd)) return Records;
            else return Records.Where(x => x.OonResolutionStatusIndicator == StatusInd);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByResolveDate(this IEnumerable<McpdOutOfNetwork> Records, string ResolveDate)
        {
            if (string.IsNullOrEmpty(ResolveDate)) return Records;
            else return Records.Where(x => string.Compare(x.OonRequestResolvedDate, ResolveDate) >= 0);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByApprovalExplain(this IEnumerable<McpdOutOfNetwork> Records, string ApprovalExplain)
        {
            if (string.IsNullOrEmpty(ApprovalExplain)) return Records;
            else return Records.Where(x => x.PartialApprovalExplanation == ApprovalExplain);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterBySpecialistNpi(this IEnumerable<McpdOutOfNetwork> Records, string SpecialistNpi)
        {
            if (string.IsNullOrEmpty(SpecialistNpi)) return Records;
            else return Records.Where(x => x.SpecialistProviderNpi == SpecialistNpi);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByOonProviderTaxonomy(this IEnumerable<McpdOutOfNetwork> Records, string ProviderTaxonomy)
        {
            if (string.IsNullOrEmpty(ProviderTaxonomy)) return Records;
            else return Records.Where(x => x.ProviderTaxonomy == ProviderTaxonomy);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressLine1(this IEnumerable<McpdOutOfNetwork> Records, string AddressLine1)
        {
            if (string.IsNullOrEmpty(AddressLine1)) return Records;
            else return Records.Where(x => x.ServiceLocationAddressLine1 == AddressLine1);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressLine2(this IEnumerable<McpdOutOfNetwork> Records, string AddressLine2)
        {
            if (string.IsNullOrEmpty(AddressLine2)) return Records;
            else return Records.Where(x => x.ServiceLocationAddressLine2 == AddressLine2);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressCity(this IEnumerable<McpdOutOfNetwork> Records, string AddressCity)
        {
            if (string.IsNullOrEmpty(AddressCity)) return Records;
            else return Records.Where(x => x.ServiceLocationCity == AddressCity);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressState(this IEnumerable<McpdOutOfNetwork> Records, string AddressState)
        {
            if (string.IsNullOrEmpty(AddressState)) return Records;
            else return Records.Where(x => x.ServiceLocationState == AddressState);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressZip(this IEnumerable<McpdOutOfNetwork> Records, string AddressZip)
        {
            if (string.IsNullOrEmpty(AddressZip)) return Records;
            else return Records.Where(x => x.ServiceLocationZip == AddressZip);
        }
        public static IEnumerable<McpdOutOfNetwork> FilterByAddressCountry(this IEnumerable<McpdOutOfNetwork> Records, string AddressCountry)
        {
            if (string.IsNullOrEmpty(AddressCountry)) return Records;
            else return Records.Where(x => x.ServiceLocationCountry == AddressCountry);
        }
        public static IEnumerable<McpdipDetail> FilterByItem(this IEnumerable<McpdipDetail> Records, string Item)
        {
            if (Item == "All") return Records;
            else return Records.Where(x => x.ResponseTarget == Item);
        }
        public static IEnumerable<McpdipDetail> FilterBySeverity(this IEnumerable<McpdipDetail> Records, string Severity)
        {
            if (Severity == "All") return Records;
            else return Records.Where(x => x.Severity == Severity);
        }
        public static IEnumerable<McpdipDetail> filterByResponseDataSource(this IEnumerable<McpdipDetail> Records, string DataSource)
        {
            if (string.IsNullOrEmpty(DataSource)) return Records;
            else return Records.Where(x => x.OriginalDataSource == DataSource);
        }
        public static IEnumerable<T> PrepareDisplay<T>(this IEnumerable<T> Records) where T : HasYearMonth
        {
            string loopYear = "";
            string loopMonth = "";
            IList<T> result = Records.ToList();
            foreach (var item in result)
            {
                if (item.RecordYear != loopYear)
                {
                    item.trClass = "parentYear";
                }
                else if (item.RecordMonth != loopMonth)
                {
                    item.trClass = "parentMonth_I" + item.RecordYear;
                }
                else
                {
                    item.trClass = "children_I" + item.RecordYear + item.RecordMonth;
                }
                loopYear = item.RecordYear;
                loopMonth = item.RecordMonth;
            }
            foreach (var item in result)
            {
                if (item.trClass.StartsWith("parent") && item.trClass != "parentYear")
                {
                    item.RecordYear = "";
                }
                else if (item.trClass.StartsWith("children"))
                {
                    item.RecordYear = "";
                    item.RecordMonth = "";
                }
            }
            return result;
        }
    }
}

