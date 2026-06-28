using NutriIndex.Ingestion.Models;
using Polly;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NutriIndex.Ingestion.Services;

public class RabbitMQEventPublisher(ILogger<RabbitMQEventPublisher> _logger) : IEventPublisher
{
    private readonly string _hostname = "localhost";
    private readonly string _queueName = "product.ingested";

    public async Task PublishAsync(ProductIngestedEvent @event)
    {
        // Resilience pipeline (Polly). Retries 3 times with exponential backoff
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"RabbitMQ connection failed. Retrying in {timeSpan.TotalSeconds}s. Attempt {retryCount}/3. Error: {exception.Message}");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var factory = new ConnectionFactory { HostName = _hostname };

            // Open RabbitMq Connection and Channel
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare queue
            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Serialize payload to bytes
            var jsonString = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(jsonString);

            // Publish message
            await channel.BasicPublishAsync(
                exchange: string.Empty, // Default exchange routing directly to queues by name
                routingKey: _queueName,
                body: body);

            _logger.LogInformation($"Successfully published ProductIngestedEvent for barcode {@event.Payload.Barcode} with CorrelationId {@event.CorrelationId}");
        });
    }
}
