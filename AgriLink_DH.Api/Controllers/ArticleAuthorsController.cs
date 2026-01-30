using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.ArticleAuthor;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleAuthorsController : ControllerBase
{
    private readonly ArticleAuthorService _authorService;
    private readonly ILogger<ArticleAuthorsController> _logger;

    public ArticleAuthorsController(
        ArticleAuthorService authorService,
        ILogger<ArticleAuthorsController> logger)
    {
        _authorService = authorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleAuthorDto>>>> GetAllAuthors()
    {
        try
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(ApiResponse<IEnumerable<ArticleAuthorDto>>.SuccessResponse(
                authors,
                "Lấy danh sách tác giả thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tác giả");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleAuthorDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách tác giả",
                500
            ));
        }
    }

    [HttpGet("verified")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleAuthorDto>>>> GetVerifiedAuthors()
    {
        try
        {
            var authors = await _authorService.GetVerifiedAuthorsAsync();
            return Ok(ApiResponse<IEnumerable<ArticleAuthorDto>>.SuccessResponse(
                authors,
                "Lấy danh sách tác giả đã xác minh thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tác giả đã xác minh");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleAuthorDto>>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ArticleAuthorDto>>> GetAuthorById(Guid id)
    {
        try
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
            {
                return NotFound(ApiResponse<ArticleAuthorDto>.NotFoundResponse(
                    $"Không tìm thấy tác giả với ID: {id}"
                ));
            }

            return Ok(ApiResponse<ArticleAuthorDto>.SuccessResponse(
                author,
                "Lấy thông tin tác giả thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tác giả có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleAuthorDto>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ArticleAuthorDto>>> CreateAuthor([FromBody] CreateArticleAuthorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleAuthorDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var author = await _authorService.CreateAuthorAsync(dto);
            return CreatedAtAction(
                nameof(GetAuthorById),
                new { id = author.Id },
                ApiResponse<ArticleAuthorDto>.CreatedResponse(
                    author,
                    "Tạo tác giả mới thành công"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi tạo tác giả: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleAuthorDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo tác giả mới");
            return StatusCode(500, ApiResponse<ArticleAuthorDto>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ArticleAuthorDto>>> UpdateAuthor(
        Guid id,
        [FromBody] UpdateArticleAuthorDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleAuthorDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var author = await _authorService.UpdateAuthorAsync(id, dto);
            return Ok(ApiResponse<ArticleAuthorDto>.SuccessResponse(
                author,
                "Cập nhật tác giả thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy tác giả có ID: {Id}", id);
            return NotFound(ApiResponse<ArticleAuthorDto>.NotFoundResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi cập nhật tác giả: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleAuthorDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật tác giả có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleAuthorDto>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAuthor(Guid id)
    {
        try
        {
            var result = await _authorService.DeleteAuthorAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa tác giả thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy tác giả có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa tác giả có ID: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }
}
