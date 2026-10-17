using Amazon.DynamoDBv2.DataModel;

namespace MediaGallery.Api.Models;

[DynamoDBTable("MediaItems")]
public class MediaItem
{
    // Klucz partycji (Partition Key) - grupuje zdjęcia danego użytkownika
    [DynamoDBHashKey]
    public string UserId { get; set; } = string.Empty;

    // Klucz sortowania (Sort Key) - unikalny identyfikator konkretnego zdjęcia
    [DynamoDBRangeKey]
    public string MediaId { get; set; } = Guid.NewGuid().ToString();

    // Nazwa przyjazna dla użytkownika, podawana w pierwszym kroku
    [DynamoDBProperty]
    public string Title { get; set; } = string.Empty;

    // Opcjonalny opis
    [DynamoDBProperty]
    public string Description { get; set; } = string.Empty;

    // Rzeczywista nazwa pliku pobierana z samego pliku na etapie uploadu
    [DynamoDBProperty]
    public string OriginalFileName { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string S3ObjectKey { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Status { get; set; } = "Pending";

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}