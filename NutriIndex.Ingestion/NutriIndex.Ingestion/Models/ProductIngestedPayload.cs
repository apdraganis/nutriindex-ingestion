namespace NutriIndex.Ingestion.Models;

public record ProductIngestedPayload(
    string Barcode,
    string UserId,
    PurchaseDetails PurchaseDetails,
    NutritionalData NutritionalData
);
