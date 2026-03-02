using Tracking.Domain.Enum;

namespace Tracking.Domain
{
    public static class ParcelSizeClassifier
    {
        private const decimal LargeSurchargeRate = 0.20m; // 20%

        public static ParcelPricing ClassifyAndPrice(
            int length,
            int width,
            int height,
            decimal baseCharge)
        {
            bool isLarge = length > 50 || width > 50 || height > 50;

            var size = isLarge ? ParcelSize.Large : ParcelSize.Standard;

            var surcharge = isLarge
                ? baseCharge * LargeSurchargeRate
                : 0m;

            return new ParcelPricing
            {
                Size = size,
                BaseCharge = baseCharge,
                SizeSurcharge = surcharge
            };
        }
    }
}
