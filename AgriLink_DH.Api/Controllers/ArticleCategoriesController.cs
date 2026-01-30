using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.ArticleCategory;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleCategoriesController : ControllerBase
{
    private readonly ArticleCategoryService _categoryService;
    private readonly ILogger<ArticleCategoriesController> _logger;

    public ArticleCategoriesController(
        ArticleCategoryService categoryService,
        ILogger<ArticleCategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy tất cả danh mục bài viết
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleCategoryDto>>>> GetAllCategories()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<ArticleCategoryDto>>.SuccessResponse(
                categories,
                "Lấy danh sách danh mục thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleCategoryDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy các danh mục đang kích hoạt
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleCategoryDto>>>> GetActiveCategories()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<ArticleCategoryDto>>.SuccessResponse(
                categories,
                "Lấy danh sách danh mục đang hoạt động thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục kích hoạt");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleCategoryDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy danh mục theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleCategoryDto>>> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse<ArticleCategoryDto>.NotFoundResponse(
                    $"Không tìm thấy danh mục với ID: {id}"
                ));
            }

            return Ok(ApiResponse<ArticleCategoryDto>.SuccessResponse(
                category,
                "Lấy thông tin danh mục thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh mục có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleCategoryDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy danh mục theo Code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleCategoryDto>>> GetCategoryByCode(ArticleCategoryType code)
    {
        try
        {
            var category = await _categoryService.GetCategoryByCodeAsync(code);
            if (category == null)
            {
                return NotFound(ApiResponse<ArticleCategoryDto>.NotFoundResponse(
                    $"Không tìm thấy danh mục với mã: {code}"
                ));
            }

            return Ok(ApiResponse<ArticleCategoryDto>.SuccessResponse(
                category,
                "Lấy thông tin danh mục thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh mục có mã: {Code}", code);
            return StatusCode(500, ApiResponse<ArticleCategoryDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Tạo mới danh mục
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ArticleCategoryDto>>> CreateCategory([FromBody] CreateArticleCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleCategoryDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var category = await _categoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.Id },
                ApiResponse<ArticleCategoryDto>.CreatedResponse(
                    category,
                    "Tạo danh mục mới thành công"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi tạo danh mục: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleCategoryDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo danh mục mới");
            return StatusCode(500, ApiResponse<ArticleCategoryDto>.ErrorResponse(
                "Đã xảy ra lỗi khi tạo danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Cập nhật danh mục
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleCategoryDto>>> UpdateCategory(
        Guid id,
        [FromBody] UpdateArticleCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleCategoryDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var category = await _categoryService.UpdateCategoryAsync(id, dto);
            return Ok(ApiResponse<ArticleCategoryDto>.SuccessResponse(
                category,
                "Cập nhật danh mục thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy danh mục có ID: {Id}", id);
            return NotFound(ApiResponse<ArticleCategoryDto>.NotFoundResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi cập nhật danh mục: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleCategoryDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật danh mục có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleCategoryDto>.ErrorResponse(
                "Đã xảy ra lỗi khi cập nhật danh mục",
                500
            ));
        }
    }

    /// <summary>
    /// Xóa danh mục
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(Guid id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa danh mục thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy danh mục có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa danh mục có ID: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi xóa danh mục",
                500
            ));
        }
    }
}
