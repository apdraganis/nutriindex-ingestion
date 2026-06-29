using NutriIndex.Ingestion.Models;

namespace NutriIndex.Ingestion.Repositories;

public interface IOutboxRepository
{
    Task SaveMessageAsync(OutboxMessage message);
}
