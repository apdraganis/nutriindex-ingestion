using MongoDB.Driver;
using NutriIndex.Ingestion.Models;
using NutriIndex.Ingestion.Services.RabbitMq;

namespace NutriIndex.Ingestion.BackgroundWorkers;

public class OutboxProcessor(
    IServiceScopeFactory _scopeFactory,
    ILogger<OutboxProcessor> _logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the Outbox processing loop.");
            }

            // Wait before polling the database again
            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor Background Service is stopping.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        // Create a scope to resolve scoped dependencies safely
        using var scope = _scopeFactory.CreateScope();

        // todo: Replace with your actual Mongo database registration or generic repository
        var mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var outboxCollection = mongoDatabase.GetCollection<OutboxMessage>("outbox");
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        
        // 1. Fetch unprocessed messages (as batch. oldest first)
        var filter = Builders<OutboxMessage>.Filter.Eq(m => m.IsProcessed, false);
        var pendingMessages = await outboxCollection
            .Find(filter)
            .SortBy(m => m.StagedAt)
            .Limit(20) // batch limits protect memory
            .ToListAsync(stoppingToken);

        if (pendingMessages.Count == 0) return;

        _logger.LogInformation("Found {Count} pending outbox messages to process.", pendingMessages.Count);

        foreach (var message in pendingMessages)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                // 2. Publish via RabbitMQ (Polly policies handle retries internally here)
                await eventPublisher.PublishAsync(message.EventData); //todo: config

                // 3. Atomically update status in MongoDB upon successful dispatch
                var update = Builders<OutboxMessage>.Update
                    .Set(m => m.IsProcessed, true)
                    .Set(m => m.ProcessedAt, DateTime.UtcNow);

                await outboxCollection.UpdateOneAsync(
                    m => m.Id == message.Id,
                    update,
                    cancellationToken: stoppingToken
                );
            }
            catch (Exception ex)
            {
                // Log and continue loop so one poisoned message doesn't block the entire queue
                _logger.LogError(ex, "Failed to process Outbox Message with ID {MessageId}.", message.Id);
            }
        }
    }
}