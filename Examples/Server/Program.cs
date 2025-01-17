using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization options
//builder.Services.Configure<JsonOptions>(options =>
//{
//    options.SerializerOptions.PropertyNamingPolicy = null; // ������������ԭʼ��Сд
//    options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // ��ѡ������ null ֵ
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
        PropertyNamingPolicy = null, // ����������ԭʼ��Сд
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // ��ѡ������ null ֵ
    };
    return Results.Json(result, options); // ʹ���Զ�������

});

app.UseStaticFiles();
app.Run();