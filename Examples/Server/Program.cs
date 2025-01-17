using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization options
//builder.Services.Configure<JsonOptions>(options =>
//{
//    options.SerializerOptions.PropertyNamingPolicy = null; // 保持属性名的原始大小写
//    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // 可选：忽略 null 值
//});

var app = builder.Build();

app.MapPost("/Report", (ReportDTO request) =>
{
    return HttpResponseDTO<bool>.Success(true, "has update.");
});

app.MapPost("/Verification", (VerifyDTO request) =>
{
    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "packages", "WinAppClient.zip");
    var packet = new FileInfo(filePath);
    var result = new List<VerificationResultDTO>
    {
        new VerificationResultDTO
        {
            RecordId = 1,
            Hash = "",
            ReleaseDate = DateTime.Now,
            Url = "http://localhost:5000/packages/WinAppClient.zip",
            Version = "1.0.2.1",
            IsForcibly = false,
            Size = packet.Length,
        }
    };
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null, // 保持属性名原始大小写
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // 可选：忽略 null 值
    };
    return Results.Json(result, options); // 使用自定义配置

});

app.UseStaticFiles();
app.Run();