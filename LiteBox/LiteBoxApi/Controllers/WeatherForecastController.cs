using Microsoft.AspNetCore.Mvc;

namespace LiteBoxApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("download/{id}")]
    public IActionResult DownloadFile(int id)
    {
        using var conn = new SqliteConnection("Data Source=filestore.db");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT file_name, is_external, content, external_path FROM file_store WHERE id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return NotFound("File not found");

        var fileName = reader.GetString(0);
        var isExternal = reader.GetInt32(1) == 1;

        if (isExternal)
        {
            var path = reader.GetString(3);
            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }
        else
        {
            var blob = reader.GetStream(2);
            return File(blob, "application/octet-stream", fileName);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file");

        var isExternal = file.Length > 20 * 1024 * 1024;
        var fileName = Path.GetFileName(file.FileName);
        var fileSize = file.Length;

        using var conn = new SqliteConnection("Data Source=filestore.db");
        conn.Open();

        var cmd = conn.CreateCommand();
        if (isExternal)
        {
            var externalPath = Path.Combine("external_files", Guid.NewGuid() + "_" + fileName);
            Directory.CreateDirectory("external_files");
            using var fs = new FileStream(externalPath, FileMode.Create);
            await file.CopyToAsync(fs);

            cmd.CommandText = @"
                INSERT INTO file_store (file_name, file_size, is_external, external_path)
                VALUES ($name, $size, 1, $path)";
            cmd.Parameters.AddWithValue("$name", fileName);
            cmd.Parameters.AddWithValue("$size", fileSize);
            cmd.Parameters.AddWithValue("$path", externalPath);
        }
        else
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            cmd.CommandText = @"
                INSERT INTO file_store (file_name, file_size, is_external, content)
                VALUES ($name, $size, 0, $blob)";
            cmd.Parameters.AddWithValue("$name", fileName);
            cmd.Parameters.AddWithValue("$size", fileSize);
            cmd.Parameters.AddWithValue("$blob", bytes);
        }

        cmd.ExecuteNonQuery();
        return Ok("Upload success");
    }

}
