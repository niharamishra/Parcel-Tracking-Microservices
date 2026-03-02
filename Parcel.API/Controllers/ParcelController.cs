using Microsoft.AspNetCore.Mvc;
using Parcel.API.Services;
using Shared.Contracts;

namespace Parcel.API.Controllers
{
    [ApiController]
    [Route("api/parcels")]
    public class ParcelController : ControllerBase
    {
        private readonly KafkaProducer _producer;

        public ParcelController(KafkaProducer producer)
        {
            _producer = producer;
        }

        // Publish Event to Kafka
        [HttpPost("events")]
        public async Task<IActionResult> PublishEvent(ParcelEvent parcelEvent)
        {
            if (parcelEvent == null)
                return BadRequest("Invalid payload.");
            parcelEvent.EventId = Guid.NewGuid();
            await _producer.PublishAsync(parcelEvent);

            return Ok("Event Published Successfully");
        }
    }
}