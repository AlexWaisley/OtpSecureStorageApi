using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OTPSecureStorageApi.Entity;
using OTPSecureStorageApi.Mappers;

namespace OTPSecureStorageApi.Controllers;

[Route("totp")]
[ApiController]
public sealed class TotpController(ILogger<TotpController> logger) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public Results<Ok<TotpDto>, NotFound> GenerateTotp(Guid id)
    {
        var totp = DataBase.Read(id);
        if (totp is not null) return TypedResults.Ok(totp.MapToDto());

        logger.LogError("Key not found");
        return TypedResults.NotFound();
    }


    [HttpGet]
    public Ok<TotpDto[]> GetAllCodes()
    {
        var totp = DataBase.ReadAll().Select(x => x.MapToDto()).ToArray();

        return TypedResults.Ok(totp);
    }

    [HttpPost]
    public Ok<TotpDto> AddTotp(TotpCreateRequest totp)
    {
            var totpEntity = totp.MapToEntity();
            DataBase.Save(totpEntity);
            var totpDto = totpEntity.MapToDto();
            return TypedResults.Ok(totpDto);
    }

    [HttpDelete("{id:guid}")]
    public Ok DeleteTotp(Guid id)
    {
            DataBase.Delete(id);
            return TypedResults.Ok();
    }
}