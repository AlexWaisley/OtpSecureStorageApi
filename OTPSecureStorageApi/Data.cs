using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using OTPSecureStorageApi.Entity;

namespace OTPSecureStorageApi;

public static class Data
{
    private static string KeyName = @"SOFTWARE\OTPStorage";
    
    public static void SaveToRegistry(string key, string value, int digitsCount, bool isDeleted = false)
    {
        if (key == "" || value == "" || digitsCount == 0)
        {
            throw new Exception("Key, value or digits count is empty");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var encryptedValue = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(value),
                null,
                DataProtectionScope.CurrentUser
            );
            using var subKey = Registry.CurrentUser.CreateSubKey(KeyName);
            if(subKey == null)
            {
                throw new Exception("Cannot create subkey");
            }

            if (subKey.GetValueNames().Contains(key))
            {
                throw new Exception("Key already exists");
            }
            subKey.SetValue(key, encryptedValue);
            using var digitsKey = subKey.CreateSubKey("DigitsCount");
            digitsKey.SetValue(key, digitsCount);
            using var isDeletedKey = subKey.CreateSubKey("IsDeleted");
            isDeletedKey.SetValue(key, isDeleted?1:0, RegistryValueKind.DWord);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
    
    public static TotpForm? ReadFromRegistry(string key)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName);
            if (subKey == null)
            {
                throw new Exception("No keys found in registry");
            }
            using var digitsKey = subKey.OpenSubKey("DigitsCount");
            using var isDeletedKey = subKey.OpenSubKey("IsDeleted");
            if(digitsKey == null || isDeletedKey == null)
            {
                throw new Exception("Registry is corrupted");
            }
            var savedObj = subKey.GetValue(key);
            if(savedObj == null)
            {
                throw new Exception("Key not found");
            }
            var encryptedValue = (byte[])savedObj;
            var value = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(encryptedValue, 
                    null, 
                    DataProtectionScope.CurrentUser));
            var digitsCount = (int)digitsKey.GetValue(key)!;
            var isDeleted = (int)isDeletedKey.GetValue(key)! == 1;
            
            var totp = new TotpForm(value, digitsCount, isDeleted, key);
            return totp.IsDeleted ? null : totp;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
    
    public static List<TotpForm> ReadAllFromRegistry()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName);
            if (subKey == null)
            {
                throw new Exception("No keys found in registry");
            }
            var keys = subKey.GetValueNames();
            var totpList = keys.Select(ReadFromRegistry).OfType<TotpForm>().ToList();
            return totpList;
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
    
    public static void DeleteFromRegistry(string key)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var subKey = Registry.CurrentUser.OpenSubKey(KeyName, true);
            if (subKey == null)
            {
                throw new Exception("No keys found in registry");
            }
            if (!subKey.GetValueNames().Contains(key))
            {
                throw new Exception("Key not found");
            }
            subKey.SetValue(key + "IsDeleted", true);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }
    }
    
}