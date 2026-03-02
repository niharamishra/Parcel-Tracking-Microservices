using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class OutForDeliveryValidator : IScanStageValidator
    {
        public string Stage => ScanStage.OutForDelivery;

        public ValidationResult Validate(ParcelContext ctx)
        {
            if (ctx.Parcel.CurrentState != ScanStage.DeliveryCenter)
                return ValidationResult.Fail("Parcel not in delivery center");

            return ValidationResult.Ok();
        }
    }
}
