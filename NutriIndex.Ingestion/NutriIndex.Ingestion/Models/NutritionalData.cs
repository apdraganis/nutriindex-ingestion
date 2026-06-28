namespace NutriIndex.Ingestion.Models;

public record NutritionalData(
    string ProductName,
    string? Brand,
    double EnergyKcalPer100g,
    double ProteinGramsPer100g,
    double? FatGramsPer100g,
    double? CarbohydratesGramsPer100g,
    double? SodiumMilligramsPer100g,
    List<string> Categories
);
