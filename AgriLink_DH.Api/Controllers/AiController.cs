using AgriLink_DH.Core.Interfaces;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Ai;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAiService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("query")]
    public async Task<ActionResult<ApiResponse<GeminiQueryResponseDto>>> Query([FromBody] GeminiQueryRequestDto request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(ApiResponse<GeminiQueryResponseDto>.ErrorResponse("Question không được để trống."));

        try
        {
            var result = await _aiService.AskQuestionAsync(request);
            return Ok(ApiResponse<GeminiQueryResponseDto>.SuccessResponse(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<GeminiQueryResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI] Query failed");
            return StatusCode(500, ApiResponse<GeminiQueryResponseDto>.ErrorResponse("Xảy ra lỗi khi gọi Gemini API."));
        }
    }
}

