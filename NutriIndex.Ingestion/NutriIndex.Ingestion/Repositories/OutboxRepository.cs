using MongoDB.Driver;
using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Repositories;

public class OutboxRepository(IMongoDatabase database) : IOutboxRepository
{
    private readonly IMongoCollection<OutboxMessage> _outboxCollection = database.GetCollection<OutboxMessage>("outbox");

    public async Task SaveMessageAsync(OutboxMessage message)
    {
        await _outboxCollection.InsertOneAsync(message);
    }
}