using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking.Domain.Enum;

namespace Tracking.Domain
{
    public sealed class ParcelPricing
    {
        public ParcelSize Size { get; init; }
        public decimal BaseCharge { get; init; }
        public decimal SizeSurcharge { get; init; }
        public decimal TotalCharge => BaseCharge + SizeSurcharge;
    }
}
