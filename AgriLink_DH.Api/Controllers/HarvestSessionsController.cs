using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.HarvestSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HarvestSessionsController : ControllerBase
{
    private readonly HarvestSessionService _harvestSessionService;
    private readonly ILogger<HarvestSessionsController> _logger;

    public HarvestSessionsController(
        HarvestSessionService harvestSessionService,
        ILogger<HarvestSessionsController> logger)
    {
        _harvestSessionService = harvestSessionService;
        _logger = logger;
    }

    [HttpGet("by-season/{seasonId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HarvestSessionDto>>>> GetBySeason(Guid seasonId)
    {
        try
        {
            var sessions = await _harvestSessionService.GetBySeasonAsync(seasonId);
            return Ok(ApiResponse<IEnumerable<HarvestSessionDto>>.SuccessResponse(sessions, "Lấy danh sách phiếu thu hoạch thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu thu hoạch");
            return StatusCode(500, ApiResponse<IEnumerable<HarvestSessionDto>>.ErrorResponse("Lỗi khi lấy danh sách phiếu thu hoạch", 500));
        }
    }

    [HttpGet("my-sessions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<HarvestSessionDto>>>> GetMySessions()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<IEnumerable<HarvestSessionDto>>.ErrorResponse("Không xác định được người dùng", 401));
            }

            var sessions = await _harvestSessionService.GetByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<HarvestSessionDto>>.SuccessResponse(sessions, "Lấy danh sách phiếu thu hoạch thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu thu hoạch của người dùng");
            return StatusCode(500, ApiResponse<IEnumerable<HarvestSessionDto>>.ErrorResponse("Lỗi khi lấy danh sách phiếu thu hoạch", 500));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<HarvestSessionDto>>> GetById(Guid id)
    {
        try
        {
            var session = await _harvestSessionService.GetByIdAsync(id);
            if (session == null)
            {
                return NotFound(ApiResponse<HarvestSessionDto>.NotFoundResponse($"Không tìm thấy phiếu thu hoạch với ID: {id}"));
            }

            return Ok(ApiResponse<HarvestSessionDto>.SuccessResponse(session, "Lấy thông tin phiếu thu hoạch thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin phiếu thu hoạch");
            return StatusCode(500, ApiResponse<HarvestSessionDto>.ErrorResponse("Lỗi khi lấy thông tin phiếu thu hoạch", 500));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HarvestSessionDto>>> Create([FromBody] CreateHarvestSessionDto dto)
    {
        try
        {
            var session = await _harvestSessionService.CreateSessionAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = session.Id },
                ApiResponse<HarvestSessionDto>.CreatedResponse(session, "Tạo phiếu thu hoạch mới thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HarvestSessionDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo phiếu thu hoạch mới");
            return StatusCode(500, ApiResponse<HarvestSessionDto>.ErrorResponse("Lỗi khi tạo phiếu thu hoạch mới", 500));
        }
    }

    [HttpPost("with-bags")]
    public async Task<ActionResult<ApiResponse<HarvestSessionDto>>> CreateWithBags([FromBody] CreateHarvestSessionWithDetailsDto dto)
    {
        try
        {
            var session = await _harvestSessionService.CreateSessionWithBagsAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = session.Id },
                ApiResponse<HarvestSessionDto>.CreatedResponse(session, "Tạo phiếu thu hoạch (kèm bao) thành công"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<HarvestSessionDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo phiếu thu hoạch kèm bao");
            return StatusCode(500, ApiResponse<HarvestSessionDto>.ErrorResponse("Lỗi khi tạo phiếu thu hoạch kèm bao", 500));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var result = await _harvestSessionService.DeleteSessionAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Xóa phiếu thu hoạch thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa phiếu thu hoạch");
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Lỗi khi xóa phiếu thu hoạch", 500));
        }
    }
}
