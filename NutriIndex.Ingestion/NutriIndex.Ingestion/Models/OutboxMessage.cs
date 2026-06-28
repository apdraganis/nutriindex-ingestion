using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NutriIndex.Ingestion.Models
{
    public class OutboxMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string EventType { get; set; } = "ProductIngestedEvent";

        // Stores the original event data directly
        public ProductIngestedEvent EventData { get; set; }

        public DateTime StagedAt { get; set; } = DateTime.UtcNow;

        // Tracks whether the background worker has processed this yet
        public bool IsProcessed { get; set; } = false;
        public DateTime? ProcessedAt { get; set; }
    }
}