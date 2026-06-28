using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Services.RabbitMq;

public interface IEventPublisher
{
    Task PublishAsync(ProductIngestedEvent @event);
}
