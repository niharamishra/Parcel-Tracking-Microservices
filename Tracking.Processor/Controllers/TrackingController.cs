using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using System.Runtime.InteropServices;
using Tracking.Processor.Entities;
using Tracking.Processor.Interfaces;
using Tracking.Processor.Response;


namespace Tracking.Processor.Controllers
{
    [ApiController]
    [Route("api/tracking")]
    public class TrackingController : ControllerBase
    {
        private readonly IParcelRepository _repository;

        public TrackingController(IParcelRepository repository)
        {
            _repository = repository;
        }

        // ---------------------------------------------------------
        // GET current parcel state
        // GET: /api/tracking/{trackingId}
        // ---------------------------------------------------------
        [HttpGet("{trackingId}")]
        public async Task<IActionResult> GetTracking(string trackingId)
        {
            if (string.IsNullOrWhiteSpace(trackingId))
                return BadRequest("Tracking ID is required.");

            var parcel = await _repository.GetByTrackingIdAsync(trackingId);

            if (parcel == null)
                return NotFound($"Tracking ID '{trackingId}' not found.");

            var response = new Parcels
            {
                ParcelId = parcel.ParcelId,
                CurrentState = parcel.CurrentState,
                LastUpdated = parcel.LastUpdated,
                SizeCategory = parcel.SizeCategory,
                BaseCharge = parcel.BaseCharge,
                Surcharge = parcel.Surcharge,
                TotalCharge = parcel.TotalCharge,
                FromLocation = parcel.FromLocation,
                ToLocation = parcel.ToLocation,
            };

            return Ok(response);
        }

        // ---------------------------------------------------------
        // GET all events for a parcel
        // GET: /api/tracking/{trackingId}/events
        // ---------------------------------------------------------
        [HttpGet("{trackingId}/events")]
        public async Task<IActionResult> GetTrackingEvents(string trackingId)
        {
            if (string.IsNullOrWhiteSpace(trackingId))
                return BadRequest("Tracking ID is required.");

            var events = await _repository.GetEventsByTrackingIdAsync(trackingId);

            if (events == null || !events.Any())
                return NotFound($"No events found for Tracking ID '{trackingId}'.");

            var response = events
                .OrderByDescending(e => e.ProcessedAt)
                .Select(e => new ParcelEventResponse
                {
                    EventType = e.EventType,
                    ParcelId = e.ParcelId,
                    Timestamp = e.ProcessedAt,
                    Location = e.Location,
                })
                .ToList();

            return Ok(response);
        }
    }
}