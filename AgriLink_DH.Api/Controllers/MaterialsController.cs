using System.Security.Claims;
using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common; // Correct namespace for ApiResponse
using AgriLink_DH.Share.DTOs.Material;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly MaterialService _materialService;
    private readonly ILogger<MaterialsController> _logger;

    public MaterialsController(MaterialService materialService, ILogger<MaterialsController> logger)
    {
        _materialService = materialService;
        _logger = logger;
    }

    [HttpGet("my-materials")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MaterialDto>>>> GetMyMaterials()
    {
        try
        {
            var userId = GetUserId();
            var materials = await _materialService.GetMyMaterialsAsync(userId);
            return Ok(ApiResponse<IEnumerable<MaterialDto>>.SuccessResponse(materials, "Lấy danh sách vật tư thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my materials");
            return StatusCode(500, ApiResponse<IEnumerable<MaterialDto>>.ErrorResponse("Lỗi hệ thống"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> CreateMaterial([FromBody] CreateMaterialDto dto)
    {
        try
        {
            var userId = GetUserId();
            var material = await _materialService.CreateMaterialAsync(userId, dto);
            return Ok(ApiResponse<MaterialDto>.SuccessResponse(material, "Thêm vật tư thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<MaterialDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating material");
            return StatusCode(500, ApiResponse<MaterialDto>.ErrorResponse("Lỗi hệ thống"));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> UpdateMaterial(Guid id, [FromBody] UpdateMaterialDto dto)
    {
        try
        {
            var userId = GetUserId();
            var material = await _materialService.UpdateMaterialAsync(userId, id, dto);
            return Ok(ApiResponse<MaterialDto>.SuccessResponse(material, "Cập nhật vật tư thành công"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<MaterialDto>.ErrorResponse("Không tìm thấy vật tư"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<MaterialDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating material");
            return StatusCode(500, ApiResponse<MaterialDto>.ErrorResponse("Lỗi hệ thống"));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMaterial(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _materialService.DeleteMaterialAsync(userId, id);
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "Xóa vật tư thành công"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy vật tư"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting material");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Lỗi hệ thống"));
        }
    }

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id)) throw new UnauthorizedAccessException("User ID not found");
        return Guid.Parse(id);
    }
}
