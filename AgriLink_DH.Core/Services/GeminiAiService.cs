using System.Text;
using System.Text.Json;
using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Core.Interfaces;
using AgriLink_DH.Share.DTOs.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgriLink_DH.Core.Services;

public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _geminiSettings;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiSettings> geminiSettings,
        ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Gemini");
        _geminiSettings = geminiSettings.Value;
        _logger = logger;
    }

    public async Task<GeminiQueryResponseDto> AskQuestionAsync(GeminiQueryRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new ArgumentException("Question không được để trống.");

        if (string.IsNullOrWhiteSpace(_geminiSettings.ApiKey))
            throw new InvalidOperationException("Gemini API key chưa cấu hình trong appsettings.");

        var chatPrompt = request.Question;
        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            chatPrompt = request.Context.Trim() + "\n" + request.Question.Trim();
        }

        var requestBody = new
        {
            contents = new[]
            {
                new 
                {
                    parts = new[] { new { text = chatPrompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                maxOutputTokens = _geminiSettings.MaxTokens
            }
        };

        var baseUrl = string.IsNullOrWhiteSpace(_geminiSettings.BaseUrl)
            ? "https://generativelanguage.googleapis.com/v1beta"
            : _geminiSettings.BaseUrl.TrimEnd('/');
            
        var url = $"{baseUrl}/models/{_geminiSettings.Model}:generateContent?key={_geminiSettings.ApiKey}";

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = content;

        var response = await _httpClient.SendAsync(httpRequest);
        var body = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("[AI] Request question={Question} URL={Url}", request.Question, url);
        _logger.LogDebug("[AI] Response body=\n{Body}", body);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Lỗi Gemini API: {body}");

        var parsed = JsonSerializer.Deserialize<JsonElement>(body);
        var answer = string.Empty;
        if (parsed.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var firstCandidate = candidates[0];
            if (firstCandidate.TryGetProperty("content", out var contentElement) && 
                contentElement.TryGetProperty("parts", out var parts))
            {
                var sb = new StringBuilder();
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var textElement))
                    {
                        sb.Append(textElement.GetString());
                    }
                }
                answer = sb.ToString();
            }
        }

        if (string.IsNullOrWhiteSpace(answer))
            answer = "Gemini trả về kết quả rỗng. Vui lòng thử lại với câu hỏi khác.";

        double usageTokens = 0;
        if (parsed.TryGetProperty("usageMetadata", out var usage) && usage.TryGetProperty("totalTokenCount", out var totalTokens))
        {
            usageTokens = totalTokens.GetDouble();
        }

        return new GeminiQueryResponseDto
        {
            Answer = answer.Trim(),
            Model = _geminiSettings.Model,
            UsageTokens = usageTokens
        };
    }
}
