using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Shared.Contracts
{
    public class ParcelEvent
    {
        [JsonIgnore]
        public Guid EventId { get; set; } = Guid.NewGuid();
        public string ParcelId { get; set; }
        public string EventType { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string Location { get; set; }
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal BasePrice { get; set; }
    }
}
