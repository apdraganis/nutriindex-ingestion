using Microsoft.AspNetCore.Mvc;
using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public class IngestionController(ILogger<IngestionController> logger) : ControllerBase
    {
        // api/v1/products/ingest
        [HttpPost("ingest")]
        public IActionResult IngestProduct([FromBody] ProductIngestedEvent request)
        {
            // asp.net core automatically validates data annotations (like [Required])
            // and returns a 400 Bad Request if the JSON doesn't match. todo: add validation

            logger.LogInformation("Successfully received event {EventId} for barcode {Barcode}",
                request.EventId, request.Payload.Barcode);

            // Returning 202 Accepted to acknowledge receipt
            return Accepted(new { message = "Payload received successfully", correlationId = request.CorrelationId });
        }
    }
}