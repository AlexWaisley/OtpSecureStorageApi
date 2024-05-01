using System.Runtime.InteropServices;

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    var path = Path.GetDirectoryName(System.Environment.ProcessPath) + "\\" + Path.GetFileName(Environment.ProcessPath);
    
    using var registry = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
    var value = registry?.GetValue("OTPSecureStorageApi");
    
    if (value == null || value.ToString() != path)
        registry?.SetValue("OTPSecureStorageApi", path);
    
}
const string originSite = "https://otpstore.pp.ua";


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("OtpStorePolicy",
        corsPolicyBuilder =>
        {
            corsPolicyBuilder.WithOrigins(originSite);
        });
});

var app = builder.Build();

app.UseCors("OtpStorePolicy");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();