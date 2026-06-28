using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Services;

public interface IEventPublisher
{
    Task PublishAsync(ProductIngestedEvent @event);
}
