using Shared.Contracts;
using Tracking.Processor.Entities;


namespace Tracking.Processor.Data
{
    public class ParcelContext
    {
        public Parcels Parcel { get; set; }
        public ParcelEvent Event { get; set; }
    }
}
