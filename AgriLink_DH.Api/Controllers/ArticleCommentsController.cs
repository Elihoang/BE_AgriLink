using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Article;
using AgriLink_DH.Share.DTOs.ArticleComment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticleCommentsController : ControllerBase
{
    private readonly ArticleCommentService _commentService;
    private readonly ILogger<ArticleCommentsController> _logger;

    public ArticleCommentsController(
        ArticleCommentService commentService,
        ILogger<ArticleCommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Lấy tất cả bình luận của bài viết
    /// </summary>
    [HttpGet("article/{articleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleCommentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleCommentDto>>>> GetCommentsByArticle(Guid articleId)
    {
        try
        {
            var comments = await _commentService.GetCommentsByArticleIdAsync(articleId);
            return Ok(ApiResponse<IEnumerable<ArticleCommentDto>>.SuccessResponse(
                comments,
                "Lấy danh sách bình luận thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bình luận cho bài viết: {ArticleId}", articleId);
            return StatusCode(500, ApiResponse<IEnumerable<ArticleCommentDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bình luận",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bình luận theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleCommentDto>>> GetCommentById(Guid id)
    {
        try
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound(ApiResponse<ArticleCommentDto>.NotFoundResponse(
                    $"Không tìm thấy bình luận với ID: {id}"
                ));
            }

            return Ok(ApiResponse<ArticleCommentDto>.SuccessResponse(
                comment,
                "Lấy thông tin bình luận thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bình luận có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleCommentDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin bình luận",
                500
            ));
        }
    }

    /// <summary>
    /// Tạo bình luận mới
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ArticleCommentDto>>> CreateComment([FromBody] CreateArticleCommentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleCommentDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<ArticleCommentDto>.ErrorResponse(
                    "Bạn cần đăng nhập để bình luận",
                    401
                ));
            }

            var comment = await _commentService.CreateCommentAsync(dto, userId);
            return CreatedAtAction(
                nameof(GetCommentById),
                new { id = comment.Id },
                ApiResponse<ArticleCommentDto>.CreatedResponse(
                    comment,
                    "Tạo bình luận thành công"
                )
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi tạo bình luận: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleCommentDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bình luận mới");
            return StatusCode(500, ApiResponse<ArticleCommentDto>.ErrorResponse(
                "Đã xảy ra lỗi khi tạo bình luận",
                500
            ));
        }
    }

    /// <summary>
    /// Cập nhật bình luận
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ArticleCommentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleCommentDto>>> UpdateComment(
        Guid id,
        [FromBody] UpdateArticleCommentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleCommentDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var userId = GetCurrentUserId();
            var comment = await _commentService.UpdateCommentAsync(id, dto, userId);
            
            return Ok(ApiResponse<ArticleCommentDto>.SuccessResponse(
                comment,
                "Cập nhật bình luận thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bình luận có ID: {Id}", id);
            return NotFound(ApiResponse<ArticleCommentDto>.NotFoundResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Không có quyền cập nhật bình luận ID: {Id}", id);
            return StatusCode(403, ApiResponse<ArticleCommentDto>.ErrorResponse(ex.Message, 403));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bình luận có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleCommentDto>.ErrorResponse(
                "Đã xảy ra lỗi khi cập nhật bình luận",
                500
            ));
        }
    }

    /// <summary>
    /// Xóa bình luận
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _commentService.DeleteCommentAsync(id, userId);
            
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa bình luận thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bình luận có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Không có quyền xóa bình luận ID: {Id}", id);
            return StatusCode(403, ApiResponse<bool>.ErrorResponse(ex.Message, 403));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bình luận có ID: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi xóa bình luận",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bình luận của user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleCommentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleCommentDto>>>> GetCommentsByUser(Guid userId)
    {
        try
        {
            var comments = await _commentService.GetCommentsByUserIdAsync(userId);
            return Ok(ApiResponse<IEnumerable<ArticleCommentDto>>.SuccessResponse(
                comments,
                "Lấy danh sách bình luận của người dùng thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bình luận của user: {UserId}", userId);
            return StatusCode(500, ApiResponse<IEnumerable<ArticleCommentDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bình luận",
                500
            ));
        }
    }
}
