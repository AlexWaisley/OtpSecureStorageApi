using OTPSecureStorageApi.Entity;

namespace OTPSecureStorageApi.Mappers;

public static class TotpMapper
{
    public static TotpDto MapToDto(this Totp totp)
    {
        return new TotpDto
        {
            Id = totp.Id,
            Name = totp.Name,
            Code = totp.GenerateCode()
        };
    }
    
    public static Totp MapToEntity(this TotpCreateRequest totp)
    {
        return new Totp
        {
            Id = Guid.NewGuid(),
            Name = totp.Name,
            SecretKey = totp.SecretKey,
            DigitsCount = totp.DigitsCount
        };
    }
}