using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class DeliveredValidator : IScanStageValidator
    {
        public string Stage => ScanStage.Delivered;

        public ValidationResult Validate(ParcelContext ctx)
        {
            if (ctx.Parcel.CurrentState != ScanStage.OutForDelivery)
                return ValidationResult.Fail("Parcel was not out for delivery");

            return ValidationResult.Ok();
        }
    }
}
