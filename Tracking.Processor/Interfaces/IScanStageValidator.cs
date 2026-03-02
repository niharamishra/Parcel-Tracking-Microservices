using Tracking.Domain.Parcels;
using Tracking.Processor.Data;

namespace Tracking.Processor.Interfaces
{
    public interface IScanStageValidator
    {
        string Stage { get; }
        ValidationResult Validate(ParcelContext context);
    }
}
