using Amazon.DynamoDBv2.DataModel;
using MediaGallery.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly string _uploadFolder;

        // Na tym etapie symulujemy autoryzację. Docelowo UserId pobierzemy z AWS Cognito.
        private const string CurrentUserId = "test-user-123";

        public MediaController(IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
            _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);
        }

        // 1. GET: Pobieranie listy multimediów
        [HttpGet]
        public async Task<IActionResult> GetMediaList()
        {
            // Zapytanie do DynamoDB o wszystkie rekordy danego użytkownika
            var conditions = new List<ScanCondition>();
            var mediaItems = await _dbContext.QueryAsync<MediaItem>(CurrentUserId).GetRemainingAsync();
            return Ok(mediaItems);
        }

        // 2. GET: Pobieranie szczegółów konkretnego pliku
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMediaDetails(string id)
        {
            var mediaItem = await _dbContext.LoadAsync<MediaItem>(CurrentUserId, id);
            if (mediaItem == null) return NotFound("Nie znaleziono pliku.");

            return Ok(mediaItem);
        }

        public class CreateMediaRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        // 3. POST: Inicjalizacja rekordu (Tytuł i opis)
        [HttpPost]
        public async Task<IActionResult> CreateMediaRecord([FromBody] CreateMediaRequest request)
        {
            if (string.IsNullOrEmpty(request.Title)) return BadRequest("Tytuł jest wymagany.");

            var mediaItem = new MediaItem
            {
                UserId = CurrentUserId,
                Title = request.Title,
                Description = request.Description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.SaveAsync(mediaItem);

            return CreatedAtAction(
                actionName: nameof(GetMediaDetails),
                routeValues: new { id = mediaItem.MediaId },
                value: new { Message = "Rekord metadanych utworzony", MediaId = mediaItem.MediaId }
            );
        }

        // 4. POST: Symulacja obciążenia (Wymóg z pkt. 10 instrukcji)
        [HttpPost("{id}/heavy-task")]
        public async Task<IActionResult> HeavyTask(string id, [FromQuery] int delayMs = 0)
        {
            // Ten endpoint użyjemy w CloudWatch do testowania alarmów
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }

            return Ok(new { Message = $"Symulacja zakończona dla wpisu {id} z opóźnieniem {delayMs} ms." });
        }

        // 5. POST (Multimedia): Przesyłanie fizycznego pliku (Upload)
        [HttpPost("{id}/upload")]
        public async Task<IActionResult> UploadMediaFile(string id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Brak pliku.");

            var mediaItem = await _dbContext.LoadAsync<MediaItem>(CurrentUserId, id);
            if (mediaItem == null) return NotFound("Najpierw utwórz rekord metadanych (POST /api/Media)");

            var filePath = Path.Combine(_uploadFolder, $"{id}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Aktualizujemy tylko pola związane bezpośrednio z plikiem, nie ruszając Tytułu i Opisu
            mediaItem.S3ObjectKey = filePath;
            mediaItem.OriginalFileName = file.FileName;
            mediaItem.Status = "Ready";

            await _dbContext.SaveAsync(mediaItem);

            return Ok(new { Message = "Plik binarny odebrany pomyślnie i połączony z metadanymi." });
        }

        // 6. GET (Multimedia): Odbieranie fizycznego pliku (Download/Stream)
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadMediaFile(string id)
        {
            var mediaItem = await _dbContext.LoadAsync<MediaItem>(CurrentUserId, id);
            if (mediaItem == null || string.IsNullOrEmpty(mediaItem.S3ObjectKey) || !System.IO.File.Exists(mediaItem.S3ObjectKey))
                return NotFound("Plik binarny nie istnieje na serwerze/S3.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(mediaItem.S3ObjectKey, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", mediaItem.OriginalFileName);
        }
    }
}