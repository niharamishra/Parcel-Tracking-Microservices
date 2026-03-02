namespace Tracking.Processor.Response
{
    public class ParcelEventResponse
    {
        public string ParcelId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string Location { get; set; }
    }
}
