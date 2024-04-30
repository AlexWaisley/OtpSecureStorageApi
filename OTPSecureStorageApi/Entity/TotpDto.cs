using System.Text.Json.Serialization;

namespace OTPSecureStorageApi.Entity;

[Serializable]
public sealed record TotpDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}