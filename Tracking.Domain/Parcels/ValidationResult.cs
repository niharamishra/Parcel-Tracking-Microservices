using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking.Domain.Parcels
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string? FailureReason { get; }

        private ValidationResult(bool valid, string? reason)
        {
            IsValid = valid;
            FailureReason = reason;
        }

        public static ValidationResult Ok() => new(true, null);
        public static ValidationResult Fail(string reason) => new(false, reason);
    }
}
