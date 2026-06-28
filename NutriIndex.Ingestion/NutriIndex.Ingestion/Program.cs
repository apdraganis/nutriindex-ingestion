using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

// 
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Grab your connection string from appsettings.json (or fallback to your local docker default)
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection")
                            ?? "mongodb://admin:adminpassword@nutriindex-ingestion-db:27017";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("NutriIndexIngestion");
});



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
