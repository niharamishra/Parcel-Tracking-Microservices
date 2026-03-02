using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tracking.Processor.Entities;

namespace Tracking.Processor.Interfaces
{
    public interface IParcelRepository
    {
        Task<Parcels?> GetByTrackingIdAsync(string trackingId);
        Task<List<ParcelEventEntity>> GetEventsByTrackingIdAsync(string trackingId);
    }
}
