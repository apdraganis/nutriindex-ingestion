namespace NutriIndex.Ingestion.Models;

public record ProductIngestedEvent(
Guid EventId,
DateTime Timestamp,
Guid CorrelationId,
ProductIngestedPayload Payload
);
