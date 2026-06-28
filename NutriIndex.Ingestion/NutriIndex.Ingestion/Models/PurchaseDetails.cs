namespace NutriIndex.Ingestion.Models;

public record PurchaseDetails(
    decimal UserEnteredPrice,
    string Currency,
    double PurchaseQuantityGrams
);
