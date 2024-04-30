using System.Text.Json.Serialization;

namespace OTPSecureStorageApi.Entity;

[Serializable]
public sealed record TotpCreateRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("secretKey")]
    public required string SecretKey { get; set; }
    [JsonPropertyName("digitsCount")]
    public required int DigitsCount { get; set; }
}