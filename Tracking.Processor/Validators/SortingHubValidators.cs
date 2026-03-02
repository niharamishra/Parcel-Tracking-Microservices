using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class SortingHubValidator : IScanStageValidator
    {
        public string Stage => ScanStage.SortingHub;

        public ValidationResult Validate(ParcelContext ctx)
        {
            if (ctx.Parcel.CurrentState != ScanStage.Collection)
                return ValidationResult.Fail("Parcel not collected yet");

            if (string.IsNullOrEmpty(ctx.Event.Location))
                return ValidationResult.Fail("Sorting hub location missing");

            return ValidationResult.Ok();
        }
    }
}
