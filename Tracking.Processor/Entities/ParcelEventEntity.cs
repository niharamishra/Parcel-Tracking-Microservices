
using System.ComponentModel.DataAnnotations;

namespace Tracking.Processor.Entities
{
    public class ParcelEventEntity
    {
        public Guid EventId { get; set; }
        public string ParcelId { get; set; }
        public string EventType { get; set; }
        public DateTime ProcessedAt { get; set; }
        public string Location { get; set; }
    }
}
