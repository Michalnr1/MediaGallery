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

    [DynamoDBProperty]
    public string FileName { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string S3ObjectKey { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Status { get; set; } = "Pending"; // Pending, Processing, Ready, Failed

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}