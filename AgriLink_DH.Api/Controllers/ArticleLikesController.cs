using AgriLink_DH.Core.Services;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Article;
using AgriLink_DH.Share.DTOs.ArticleLike;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/articles/{articleId:guid}/likes")]
public class ArticleLikesController : ControllerBase
{
    private readonly ArticleLikeService _likeService;
    private readonly ILogger<ArticleLikesController> _logger;

    public ArticleLikesController(
        ArticleLikeService likeService,
        ILogger<ArticleLikesController> logger)
    {
        _likeService = likeService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Like bài viết
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ArticleLikeDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ArticleLikeDto>>> LikeArticle(Guid articleId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<ArticleLikeDto>.ErrorResponse(
                    "Bạn cần đăng nhập để thích bài viết",
                    401
                ));
            }

            var like = await _likeService.LikeArticleAsync(new LikeArticleDto { ArticleId = articleId }, userId);
            return CreatedAtAction(
                nameof(CheckUserLiked),
                new { articleId },
                ApiResponse<ArticleLikeDto>.CreatedResponse(
                    like,
                    "Thích bài viết thành công"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "User {UserId} đã like bài viết {ArticleId}", GetCurrentUserId(), articleId);
            return BadRequest(ApiResponse<ArticleLikeDto>.ErrorResponse(ex.Message, 400));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết: {ArticleId}", articleId);
            return NotFound(ApiResponse<ArticleLikeDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi like bài viết: {ArticleId}", articleId);
            return StatusCode(500, ApiResponse<ArticleLikeDto>.ErrorResponse(
                "Đã xảy ra lỗi khi thích bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Unlike bài viết
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> UnlikeArticle(Guid articleId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(
                    "Bạn cần đăng nhập",
                    401
                ));
            }

            var result = await _likeService.UnlikeArticleAsync(articleId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Bỏ thích bài viết thành công"
            ));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy like: {ArticleId} - {UserId}", articleId, GetCurrentUserId());
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi unlike bài viết: {ArticleId}", articleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi bỏ thích bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Kiểm tra user đã like bài viết chưa
    /// </summary>
    [HttpGet("check")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUserLiked(Guid articleId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(
                    false,
                    "Chưa đăng nhập"
                ));
            }

            var hasLiked = await _likeService.HasUserLikedArticleAsync(articleId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                hasLiked,
                hasLiked ? "Đã thích" : "Chưa thích"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra like status: {ArticleId}", articleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy danh sách users đã like bài viết
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetLikeCount(Guid articleId)
    {
        try
        {
            var count = await _likeService.GetArticleLikeCountAsync(articleId);
            return Ok(ApiResponse<int>.SuccessResponse(
                count,
                "Lấy số lượng likes thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy like count cho bài viết: {ArticleId}", articleId);
            return StatusCode(500, ApiResponse<int>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }
}
