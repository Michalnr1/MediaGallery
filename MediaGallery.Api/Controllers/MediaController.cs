using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    // 1. GET: Pobieranie listy multimediów
    [HttpGet]
    public IActionResult GetMediaList()
    {
        // TODO: Pobranie z DynamoDB (użyjemy na razie stałego UserId do testów)
        return Ok(new[] { "Zdjęcie1", "Zdjęcie2" });
    }

    // 2. GET: Pobieranie szczegółów / pliku
    [HttpGet("{id}")]
    public IActionResult GetMediaDetails(string id)
    {
        // TODO: Zwrócenie metadanych lub wygenerowanego URL'a z S3
        return Ok(new { MediaId = id, Status = "Ready" });
    }

    // 3. POST/PUT: Przesyłanie plików multimedialnych
    [HttpPost("upload")]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Brak pliku.");

        // TODO: Zapis lokalny do testów, docelowo upload prosto do AWS S3
        return Ok(new { Message = "Plik odebrany", FileName = file.FileName });
    }

    // 4. POST: Symulacja obciążenia (Wymaganie do CloudWatch)
    [HttpPost("heavy-task")]
    public async Task<IActionResult> HeavyTask([FromQuery] int delayMs = 0)
    {
        if (delayMs > 0)
        {
            // Symuluje pogorszenie wydajności aplikacji na potrzeby zdefiniowania 
            // metryk i alarmów CloudWatch w dalszej części projektu.
            await Task.Delay(delayMs);
        }

        return Ok(new { Message = $"Zadanie zakończone z opóźnieniem {delayMs} ms." });
    }
}