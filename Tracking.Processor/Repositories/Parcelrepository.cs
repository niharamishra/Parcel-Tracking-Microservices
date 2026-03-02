using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using Tracking.Processor.Entities;
using Tracking.Processor.Interfaces;

namespace Parcel.Repositories
{
    public class ParcelRepository : IParcelRepository
    {
        private readonly ParcelDbContext _context;

        public ParcelRepository(ParcelDbContext context)
        {
            _context = context;
        }
        // ---------------------------------------------------------
        // Get current parcel
        // ---------------------------------------------------------
        public async Task<Parcels?> GetByTrackingIdAsync(string trackingId)
        {
            return await _context.Parcels
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(p => p.ParcelId == trackingId);
        }
        // ---------------------------------------------------------
        // Get all events by tracking ID
        // ---------------------------------------------------------
        public async Task<List<ParcelEventEntity>> GetEventsByTrackingIdAsync(string trackingId)
        {
            if (string.IsNullOrWhiteSpace(trackingId))
                return new List<ParcelEventEntity>();

            return await _context.ParcelEvents
                .AsNoTracking()
                .Where(e => e.ParcelId == trackingId)
                .OrderBy(e => e.ProcessedAt)   // oldest → latest
                .ToListAsync();
        }
    }
}