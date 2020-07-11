using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mcpdipData
{
    public interface HasAttributes
    {
        string TradingPartnerCode { get; }
        string PlanCode { get; }
        string Cin { get; }
        string DataSource { get; }
    }
    public interface HasYearMonth
    {
        string trClass { get; set; }
        string RecordYear { get; set; }
        string RecordMonth { get; set; }
    }
    public interface HasMcpdAttributes
    {
        string RecordType { get; }
        string BenefitType { get; }
    }
}
