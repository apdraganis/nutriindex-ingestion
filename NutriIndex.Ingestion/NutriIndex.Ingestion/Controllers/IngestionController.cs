using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class IngestionController : ControllerBase
    {
        IMongoCollection<OutboxMessage> _outboxCollection;
        ILogger<IngestionController> _logger;

        public IngestionController(ILogger<IngestionController> logger, IMongoDatabase _db)
        {
            _logger = logger;
            _outboxCollection = _db.GetCollection<OutboxMessage>("outbox");
        }

        // api/v1/products/ingest
        [HttpPost("ingest")]
        public async Task<IActionResult> IngestProduct([FromBody] ProductIngestedEvent request)
        {
            // asp.net core automatically validates data annotations (like [Required])
            // and returns a 400 Bad Request if the JSON doesn't match. todo: add validation

            _logger.LogInformation("Successfully received event {EventId} for barcode {Barcode}",
                request.EventId, request.Payload.Barcode);

            // 1. Wrap request payload into Outbox message, for mongodb
            var outboxMessage = new OutboxMessage
            {
                EventData = request
            };

            // 2. Insert it into MongoDb
            try
            {
                await _outboxCollection.InsertOneAsync(outboxMessage);
                _logger.LogInformation("Successfully staged message {EventId} into the Outbox.", request.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist event {EventId} to MongoDB.", request.EventId);
                return StatusCode(500, "Internal error saving record.");
            }

            // Returning 202 Accepted to acknowledge receipt
            return Accepted(new { message = "Payload received successfully", correlationId = request.CorrelationId });
        }
    }
}