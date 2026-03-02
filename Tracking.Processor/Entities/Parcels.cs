using System.Text.Json.Serialization;

namespace Tracking.Processor.Entities
{
    public class Parcels
    {
        public string ParcelId { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string CurrentState { get; set; }
        public DateTime LastUpdated { get; set; }
        [JsonIgnore]
        public int Version { get; set; }

        public string SizeCategory { get; set; } 
        public decimal BaseCharge { get; init; }
        public decimal Surcharge { get; init; }
        public decimal TotalCharge { get; init; }
    }
}