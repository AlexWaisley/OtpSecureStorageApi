namespace OTPSecureStorageApi.Entity;

public sealed record Totp
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string SecretKey { get; set; }
    public required int DigitsCount { get; set; }
}