using System.Text.Json;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Disease;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

/// <summary>
/// Chẩn đoán bệnh cây trồng bằng AI (Roboflow ResNet18 Classification).
/// Model: coffee-aejli-rsyrz/1 — 95.5% accuracy
/// </summary>
[ApiController]
[Route("api/diagnose")]
public class DiagnoseController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DiagnoseController> _logger;

    // Roboflow config
    private const string ApiKey  = "i4CgC0ZaySLzfoYhNocv";
    private const string ModelId = "coffee-aejli-rsyrz/1";
    private const string BaseUrl = "https://serverless.roboflow.com"; // đúng URL từ dashboard

    // Map tên class → tiếng Việt + lời khuyên (Case-Insensitive)
    private static readonly Dictionary<string, (string Label, string Advice)> DiseaseMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["leaf_rust"]       = ("Gỉ sắt lá (Rust)",    "Phun thuốc gốc đồng (Bordeaux) hoặc Mancozeb. Tỉa bỏ lá bị nặng. Tránh tưới nước lên tán lá."),
        ["rust"]            = ("Gỉ sắt lá",           "Phun thuốc gốc đồng (Bordeaux) hoặc Mancozeb. Tỉa bỏ lá bị nặng. Tránh tưới nước lên tán lá."),
        ["brown_eye_spot"]  = ("Đốm mắt nâu",         "Bổ sung dinh dưỡng (phân Kali, Boron). Phun Carbendazim hoặc Thiophanate-methyl."),
        ["cercospora"]      = ("Đốm lá Cercospora",   "Phun thuốc diệt nấm phổ rộng chứa Mancozeb hoặc Difenoconazole."),
        ["anthracnose"]     = ("Thán thư",            "Cắt tỉa và tiêu hủy cành/quả bị bệnh. Phun Azoxystrobin hoặc Propiconazole."),
        ["phoma"]           = ("Bệnh Phoma",          "Bệnh do nấm, phát triển mạnh khi trời lạnh mưa. Cắt cành chớm bệnh, phun Validamycin."),
        ["miner"]           = ("Sâu vẽ bùa",          "Phun thuốc trừ sâu nội hấp sớm khi mới nhú lộc non. Cắt bỏ lá bệnh."),
        ["healthy"]         = ("Khỏe mạnh (Healthy)", "Cây đang phát triển tốt. Duy trì chế độ chăm sóc hiện tại."),
        ["unhealthy"]       = ("Có dấu hiệu bệnh",    "Cây đang có dấu hiệu nhiễm bệnh. Cần đem mẫu lá đến chuyên gia nông nghiệp hoặc nhà thuốc BVTV để kiểm tra chính xác loại nấm/sâu bọ."),
    };

    public DiagnoseController(IHttpClientFactory httpClientFactory, ILogger<DiagnoseController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    /// <summary>
    /// Upload ảnh lá cây → AI trả về loại bệnh + lời khuyên xử lý.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<DiagnoseResultDto>>> Diagnose(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest(ApiResponse<DiagnoseResultDto>.ErrorResponse("Vui lòng chọn ảnh."));

        if (image.Length > 10 * 1024 * 1024)
            return BadRequest(ApiResponse<DiagnoseResultDto>.ErrorResponse("Ảnh không được vượt quá 10MB."));

        try
        {
            // Convert ảnh → base64
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var base64Image = Convert.ToBase64String(ms.ToArray());

            // Gọi Roboflow Hosted API
            var url = $"{BaseUrl}/{ModelId}?api_key={ApiKey}&name={image.FileName}";
            _logger.LogInformation("[DIAGNOSE] Calling Roboflow API → Model={ModelId} File={File} Size={Size}KB",
                ModelId, image.FileName, image.Length / 1024);

            var content  = new StringContent(base64Image);
            var response = await _httpClient.PostAsync(url, content);
            var body     = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[DIAGNOSE] Response [{Status}]: {Body}",
                (int)response.StatusCode, body);

            if (!response.IsSuccessStatusCode)
                return StatusCode(502, ApiResponse<DiagnoseResultDto>.ErrorResponse($"Lỗi Roboflow API: {body}", 502));

            // Parse kết quả Roboflow
            // Response format: { "top": "leaf_rust", "confidence": 0.95, "predictions": [...] }
            var json        = JsonSerializer.Deserialize<JsonElement>(body);
            var topClass    = json.TryGetProperty("top", out var t) ? t.GetString() ?? "unknown" : "unknown";
            var confidence  = json.TryGetProperty("confidence", out var c) ? c.GetDouble() : 0.0;

            // Lookup tiếng Việt
            var (label, advice) = DiseaseMap.TryGetValue(topClass, out var info)
                ? info
                : ("Không xác định", "Không nhận diện được bệnh. Vui lòng chụp lại ảnh rõ hơn.");

            var result = new DiagnoseResultDto
            {
                Disease      = topClass,
                DiseaseLabel = label,
                Confidence   = confidence,
                Advice       = advice,
            };

            _logger.LogInformation("[DIAGNOSE] Result → {Label} ({Confidence:P1})", label, confidence);
            return Ok(ApiResponse<DiagnoseResultDto>.SuccessResponse(result, $"Phát hiện: {label}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DIAGNOSE] Error");
            return StatusCode(500, ApiResponse<DiagnoseResultDto>.ErrorResponse("Lỗi xử lý. Vui lòng thử lại.", 500));
        }
    }
}
