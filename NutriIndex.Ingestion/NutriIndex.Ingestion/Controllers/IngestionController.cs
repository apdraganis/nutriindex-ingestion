using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NutriIndex.Ingestion.Models;
using NutriIndex.Ingestion.Repositories;

namespace NutriIndex.Ingestion.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class IngestionController : ControllerBase
    {
        private readonly IOutboxRepository _outboxRepository;
        ILogger<IngestionController> _logger;

        public IngestionController(ILogger<IngestionController> logger, IOutboxRepository outboxRepository)
        {
            _logger = logger;
            _outboxRepository = outboxRepository;
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
            var outboxMessage = new OutboxMessage { EventData = request };

            // 2. Insert it into MongoDb. Background Worker will try to publish it.
            try
            {
                await _outboxRepository.SaveMessageAsync(outboxMessage);
                _logger.LogInformation("Successfully staged message {EventId} into the Outbox.", request.EventId);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // 409 Conflict indicates the resource/event has already been processed or received
                _logger.LogWarning("Duplicate event detected and blocked: {EventId}", request.EventId);
                return Conflict(new { message = "This event has already been processed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist event {EventId} to MongoDB.", request.EventId);
                return StatusCode(500, "Internal error saving record.");
            }

            // 3. Return 202 Accepted to acknowledge receipt
            return Accepted(new { message = "Payload received successfully", correlationId = request.CorrelationId });
        }
    }
}