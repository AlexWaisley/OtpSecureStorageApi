namespace OTPSecureStorageApi.Entity;

public class TotpForm
{
    public string Name { get; set; }
    public string Key { get; set; }
    public int DigitsCount { get; set; }
    public bool IsDeleted { get; set; }
    
    public TotpForm(string key, int digitsCount, bool isDeleted = false, string name = "Zero")
    {
        Name = name;
        Key = key;
        DigitsCount = digitsCount;
        IsDeleted = isDeleted;
    }
}