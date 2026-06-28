namespace NutriIndex.Ingestion.Models;

public record NutritionalData(
    string ProductName,
    string? Brand,
    double EnergyKcalPer100g,
    double ProteinGramsPer100g
);
