using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly CloudinaryService _cloudinaryService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(CloudinaryService cloudinaryService, ILogger<UploadController> logger)
    {
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Upload ảnh lên Cloudinary. Trả về URL của ảnh đã upload.
    /// Tất cả ảnh của dự án đều lưu vào folder 'agrilink' trên Cloudinary.
    /// </summary>
    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file)
    {
        const string folder = "agrilink";

        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("Vui lòng chọn file ảnh."));

            var imageUrl = await _cloudinaryService.UploadImageAsync(file, folder);  // folder = "agrilink"
            return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Upload ảnh thành công"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload ảnh lên Cloudinary");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Lỗi hệ thống khi upload ảnh."));
        }
    }
}
