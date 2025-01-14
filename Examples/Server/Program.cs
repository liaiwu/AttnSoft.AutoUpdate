using ServerSample.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/Report", (ReportDTO request) =>
{
    return HttpResponseDTO<bool>.Success(true,"has update.");
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
            //Hash = "5a6b90b388d214c6fe69c84b0ca5e526c749c9fbe774d3b801da567ab24d0824",
            Hash = "",
            ReleaseDate = DateTime.Now,
            Url = "http://localhost:5000/packages/WinAppClient.zip",
            Version = "1.0.0.1",
            IsForcibly = false,
            Size = packet.Length,
        }
    };
    return result;
});

app.UseStaticFiles();
app.Run();