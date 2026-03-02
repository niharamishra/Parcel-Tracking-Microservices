using Tracking.Domain.Parcels;
using Tracking.Processor.Data;
using Tracking.Processor.Interfaces;

namespace Tracking.Processor.Validators
{
    public class ScanStageValidatorRegistry
    {
        private readonly Dictionary<string, IScanStageValidator> _validators;

        public ScanStageValidatorRegistry(IEnumerable<IScanStageValidator> validators)
        {
            _validators = validators.ToDictionary(v => v.Stage);
        }

        public ValidationResult Validate(string stage, ParcelContext context)
        {
            if (!_validators.TryGetValue(stage, out var validator))
                return ValidationResult.Fail($"No validator registered for {stage}");

            return validator.Validate(context);
        }
    }
}
