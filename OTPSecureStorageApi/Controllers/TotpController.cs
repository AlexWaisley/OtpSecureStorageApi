using Microsoft.AspNetCore.Mvc;
using OTPSecureStorageApi.Entity;

namespace OTPSecureStorageApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TotpController : ControllerBase
    {
        [HttpGet("{key}")]
        public Task<ActionResult<string>> GetCode(string key)
        {
            try
            {
                var totp = Data.ReadFromRegistry(key);
                if(totp == null)
                {
                    return Task.FromResult<ActionResult<string>>(NotFound("Key not found"));
                }
                return Task.FromResult<ActionResult<string>>(Ok(Totp.GetCode(totp.Key,totp.DigitsCount)));
            }
            catch (Exception e)
            {
                return Task.FromResult<ActionResult<string>>(NotFound(e.Message));
            }
        }

        [HttpGet]
        public Task<ActionResult<List<TotpForm>>> GetAllCodes()
        {
            try
            {
                var totp = Data.ReadAllFromRegistry();
                totp.ForEach(x=>x.Key = Totp.GetCode(x.Key, x.DigitsCount));
                return Task.FromResult<ActionResult<List<TotpForm>>>(Ok(totp));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult<ActionResult<List<TotpForm>>>(NotFound(e.Message));
            }
        }

        [HttpPost]
        public Task<ActionResult<List<TotpForm>>> AddTotp(TotpForm totp)
        {
            try
            {
                Data.SaveToRegistry(totp.Name, totp.Key, totp.DigitsCount);
                var values = Data.ReadAllFromRegistry();
                return Task.FromResult<ActionResult<List<TotpForm>>>(Ok(values));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult<ActionResult<List<TotpForm>>>(NotFound(e.Message));
            }
        }
        
        [HttpDelete("{key}")]
        public Task<ActionResult<List<TotpForm>>> DeleteTotp(string key)
        {
            try
            {
                Data.DeleteFromRegistry(key);
                var values = Data.ReadAllFromRegistry();
                return Task.FromResult<ActionResult<List<TotpForm>>>(Ok(values));
            }
            catch (Exception e)
            {
                return Task.FromResult<ActionResult<List<TotpForm>>>(NotFound(e.Message));
            }
        }
    }
}