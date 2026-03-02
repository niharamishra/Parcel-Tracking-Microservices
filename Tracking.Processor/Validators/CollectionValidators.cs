using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class CollectionValidator : IScanStageValidator
    {
        public string Stage => ScanStage.Collection;

        public ValidationResult Validate(ParcelContext ctx)
        {
            if (ctx.Parcel.CurrentState != "COLLECTED")
                return ValidationResult.Fail("Parcel must be CREATED before collection");

            return ValidationResult.Ok();
        }
    }
}
