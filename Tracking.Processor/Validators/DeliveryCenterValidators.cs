using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class DeliveryCenterValidator : IScanStageValidator
    {
        public string Stage => ScanStage.DeliveryCenter;

        public ValidationResult Validate(ParcelContext ctx)
        {
            if (ctx.Parcel.CurrentState != ScanStage.SortingHub)
                return ValidationResult.Fail("Parcel must arrive from sorting hub");

            return ValidationResult.Ok();
        }
    }
}
