using System.Security.Cryptography;

namespace OTPSecureStorageApi.Entity;

public static class Totp
{
    private static byte[] Extract31(byte[] hmac, int i)
    {
        var resBytes = new byte[4];
        Array.Copy(hmac, i, resBytes, 0, 4);
        resBytes[0] &= 0x7f;
        if (BitConverter.IsLittleEndian)
            Array.Reverse(resBytes);
        return resBytes;
    }
    private static byte[] Truncate(byte[] hmac)
    {
        var offset = hmac[^1]&0x0f;
        var res = Extract31(hmac, offset);
        return res;
    }
    private static long HotpCalc(string key, long counter)
    {
        using var hmac = new HMACSHA1(Base32Decode(key));
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }
        
        var res = Truncate(hmac.ComputeHash(counterBytes));
        var code = BitConverter.ToInt32(res, 0);
        return code;
    }
    
    public static string GetCode(string key, int digitsCount = 6)
    {
        var date = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var counter = date/30;
        
        var res = (int)(HotpCalc(key,counter)%Math.Pow(10, digitsCount));
        return res.ToString($"D{digitsCount}");
    }
    
    private static byte[] Base32Decode(string base32Encoded)
    {
        const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        base32Encoded = base32Encoded.Trim().Replace(" ", "").ToUpper();
        var bytes = new byte[base32Encoded.Length * 5 / 8];

        var position = 0;
        var buffer = 0;
        var bitsRemaining = 0;

        foreach (var index in base32Encoded.Select(c => base32Chars.IndexOf(c)).Where(index => index >= 0))
        {
            buffer <<= 5;
            buffer |= index;
            bitsRemaining += 5;

            if (bitsRemaining < 8) continue;
            bytes[position++] = (byte)(buffer >> (bitsRemaining - 8));
            bitsRemaining -= 8;
        }
        return bytes;
    }
}