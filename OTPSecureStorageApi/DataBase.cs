using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;
using OTPSecureStorageApi.Entity;

namespace OTPSecureStorageApi;

public static class DataBase
{
    private static readonly string KeyName = InitStorageLocation();

    private static string InitStorageLocation()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "SOFTWARE\\OTPSecureStorage";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "./otpstorage";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "./otpstorage";
        }

        throw new PlatformNotSupportedException();
    }


    public static void Save(Totp totp)
    {
        var jsonValue = JsonSerializer.Serialize(totp);
        var key = totp.Id.ToString();
        var encryptedKey = ProtectData(jsonValue);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.CreateSubKey(KeyName);
            if (subKey is null)
            {
                throw new Exception("Cannot create subkey");
            }

            subKey.SetValue(key, encryptedKey);
        }
        else
        {
            if (!Directory.Exists(KeyName))
            {
                Directory.CreateDirectory(KeyName);
            }

            File.WriteAllText(Path.Join(KeyName, key), jsonValue);
        }
    }

    private static byte[] ProtectData(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        }

        return bytes;
    }

    public static Totp? Read(Guid key)
    {
        var keyString = key.ToString();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName);

            var savedObj = subKey?.GetValue(keyString);
            if (savedObj is not byte[] encryptedValue)
            {
                return null;
            }

            var value = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(encryptedValue,
                    null,
                    DataProtectionScope.CurrentUser));

            return JsonSerializer.Deserialize<Totp>(value);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (!Directory.Exists(KeyName))
            {
                return null;
            }

            var value = File.ReadAllText(Path.Join(KeyName, keyString));
            return JsonSerializer.Deserialize<Totp>(value);
        }

        throw new PlatformNotSupportedException();
    }

    public static void Delete(Guid key)
    {
        var keyString = key.ToString();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName, true);
            subKey?.DeleteValue(keyString);
        }
        else
        {
            File.Delete(Path.Join(KeyName, keyString));
        }
    }

    private static IEnumerable<Guid> ReadIds()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName);
            if (subKey is null)
            {
                yield break;
            }

            var keys = subKey.GetValueNames();
            foreach (var key in keys)
            {
                yield return Guid.Parse(key);
            }

            yield break;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (!Directory.Exists(KeyName))
            {
                yield break;
            }

            var files = Directory.GetFiles(KeyName);
            foreach (var file in files)
            {
                var key = Path.GetFileNameWithoutExtension(file);
                yield return Guid.Parse(key);
            }

            yield break;
        }

        throw new PlatformNotSupportedException();
    }

    public static IEnumerable<Totp> ReadAll()
    {
        var keys = ReadIds();
        foreach (var keyId in keys)
        {
            if (Read(keyId) is { } totp)
            {
                yield return totp;
            }
        }
    }
}