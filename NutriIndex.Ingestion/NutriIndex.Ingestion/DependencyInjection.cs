using MongoDB.Driver;
using NutriIndex.Ingestion.Models;
using NutriIndex.Ingestion.Repositories;

namespace NutriIndex.Ingestion;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoConnection")
                               ?? "mongodb://admin:adminpassword@nutriindex-ingestion-db:27017";

        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase("NutriIndexIngestion");

            // Get collection reference
            var outboxCollection = database.GetCollection<OutboxMessage>("outbox");

            // Define ascending index on the nested EventId field
            var indexKeysDefinition = Builders<OutboxMessage>.IndexKeys.Ascending(m => m.EventData.EventId);

            // Make it Unique
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexModel = new CreateIndexModel<OutboxMessage>(indexKeysDefinition, indexOptions);

            // Execute creation asynchronously (it's safe to call it multiple times, Mongo skips if it exists)
            outboxCollection.Indexes.CreateOne(indexModel);

            return database;
        });

        // Register the repository contract and its implementation
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        return services;
    }
}