using AgriLink_DH.Core.Services;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Article;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly ArticleService _articleService;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        ArticleService articleService,
        ILogger<ArticlesController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Lấy tất cả bài viết với pagination và filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAllArticles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? authorId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "publishedAt",
        [FromQuery] bool? isFeatured = null)
    {
        try
        {
            // Parse status if provided
            ArticleStatus? articleStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ArticleStatus>(status, true, out var parsedStatus))
            {
                articleStatus = parsedStatus;
            }

            // Get filtered articles
            IEnumerable<ArticleDto> articles;
            
            if (articleStatus.HasValue)
            {
                articles = await _articleService.GetArticlesByStatusAsync(articleStatus.Value);
            }
            else
            {
                articles = await _articleService.GetAllArticlesAsync();
            }

            // Apply filters
            var filteredArticles = articles.AsQueryable();

            if (categoryId.HasValue)
            {
                filteredArticles = filteredArticles.Where(a => a.CategoryId == categoryId.Value);
            }

            if (authorId.HasValue)
            {
                filteredArticles = filteredArticles.Where(a => a.AuthorId == authorId.Value);
            }

            if (isFeatured.HasValue)
            {
                filteredArticles = filteredArticles.Where(a => a.IsFeatured == isFeatured.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                filteredArticles = filteredArticles.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchLower))
                );
            }

            // Apply sorting
            filteredArticles = sortBy?.ToLower() switch
            {
                "viewcount" => filteredArticles.OrderByDescending(a => a.ViewCount),
                "likecount" => filteredArticles.OrderByDescending(a => a.LikeCount),
                "commentcount" => filteredArticles.OrderByDescending(a => a.CommentCount),
                "createdat" => filteredArticles.OrderByDescending(a => a.CreatedAt),
                _ => filteredArticles.OrderByDescending(a => a.PublishedAt) // Default: publishedAt
            };

            // Pagination
            var totalCount = filteredArticles.Count();
            var items = filteredArticles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(ApiResponse<object>.SuccessResponse(
                result,
                "Lấy danh sách bài viết thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết đã xuất bản (Public)
    /// </summary>
    [HttpGet("published")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleListItemDto>>>> GetPublishedArticles()
    {
        try
        {
            var articles = await _articleService.GetPublishedArticlesAsync();
            return Ok(ApiResponse<IEnumerable<ArticleListItemDto>>.SuccessResponse(
                articles,
                "Lấy danh sách bài viết đã xuất bản thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết đã xuất bản");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleListItemDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết nổi bật
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleListItemDto>>>> GetFeaturedArticles([FromQuery] int count = 10)
    {
        try
        {
            var articles = await _articleService.GetFeaturedArticlesAsync(count);
            return Ok(ApiResponse<IEnumerable<ArticleListItemDto>>.SuccessResponse(
                articles,
                "Lấy danh sách bài viết nổi bật thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết nổi bật");
            return StatusCode(500, ApiResponse<IEnumerable<ArticleListItemDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết theo danh mục
    /// </summary>
    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleDto>>>> GetArticlesByCategory(Guid categoryId)
    {
        try
        {
            var articles = await _articleService.GetArticlesByCategoryIdAsync(categoryId);
            return Ok(ApiResponse<IEnumerable<ArticleDto>>.SuccessResponse(
                articles,
                "Lấy danh sách bài viết theo danh mục thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết theo danh mục: {CategoryId}", categoryId);
            return StatusCode(500, ApiResponse<IEnumerable<ArticleDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết theo tác giả
    /// </summary>
    [HttpGet("by-author/{authorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleDto>>>> GetArticlesByAuthor(Guid authorId)
    {
        try
        {
            var articles = await _articleService.GetArticlesByAuthorIdAsync(authorId);
            return Ok(ApiResponse<IEnumerable<ArticleDto>>.SuccessResponse(
                articles,
                "Lấy danh sách bài viết theo tác giả thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết theo tác giả: {AuthorId}", authorId);
            return StatusCode(500, ApiResponse<IEnumerable<ArticleDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết theo trạng thái (Admin)
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArticleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArticleDto>>>> GetArticlesByStatus(ArticleStatus status)
    {
        try
        {
            var articles = await _articleService.GetArticlesByStatusAsync(status);
            return Ok(ApiResponse<IEnumerable<ArticleDto>>.SuccessResponse(
                articles,
                "Lấy danh sách bài viết theo trạng thái thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết theo trạng thái: {Status}", status);
            return StatusCode(500, ApiResponse<IEnumerable<ArticleDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> GetArticleById(Guid id, [FromQuery] bool incrementView = false)
    {
        try
        {
            var article = await _articleService.GetArticleByIdAsync(id, incrementView);
            if (article == null)
            {
                return NotFound(ApiResponse<ArticleDto>.NotFoundResponse(
                    $"Không tìm thấy bài viết với ID: {id}"
                ));
            }

            return Ok(ApiResponse<ArticleDto>.SuccessResponse(
                article,
                "Lấy thông tin bài viết thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bài viết có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy bài viết theo Slug
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> GetArticleBySlug(string slug, [FromQuery] bool incrementView = true)
    {
        try
        {
            var article = await _articleService.GetArticleBySlugAsync(slug, incrementView);
            if (article == null)
            {
                return NotFound(ApiResponse<ArticleDto>.NotFoundResponse(
                    $"Không tìm thấy bài viết với slug: {slug}"
                ));
            }

            return Ok(ApiResponse<ArticleDto>.SuccessResponse(
                article,
                "Lấy thông tin bài viết thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bài viết có slug: {Slug}", slug);
            return StatusCode(500, ApiResponse<ArticleDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Tạo mới bài viết
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> CreateArticle([FromBody] CreateArticleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var currentUserId = GetCurrentUserId();
            var article = await _articleService.CreateArticleAsync(dto, currentUserId);
            return CreatedAtAction(
                nameof(GetArticleById),
                new { id = article.Id },
                ApiResponse<ArticleDto>.CreatedResponse(
                    article,
                    "Tạo bài viết mới thành công"
                )
            );
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi tạo bài viết: {Message}", ex.Message);
            return BadRequest(ApiResponse<ArticleDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài viết mới");
            return StatusCode(500, ApiResponse<ArticleDto>.ErrorResponse(
                "Đã xảy ra lỗi khi tạo bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Cập nhật bài viết
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> UpdateArticle(
        Guid id,
        [FromBody] UpdateArticleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ArticleDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            var currentUserId = GetCurrentUserId();
            var article = await _articleService.UpdateArticleAsync(id, dto, currentUserId);
            return Ok(ApiResponse<ArticleDto>.SuccessResponse(
                article,
                "Cập nhật bài viết thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<ArticleDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài viết có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleDto>.ErrorResponse(
                "Đã xảy ra lỗi khi cập nhật bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Xuất bản bài viết
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ArticleDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ArticleDto>>> PublishArticle(Guid id)
    {
        try
        {
            var article = await _articleService.PublishArticleAsync(id);
            return Ok(ApiResponse<ArticleDto>.SuccessResponse(
                article,
                "Xuất bản bài viết thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<ArticleDto>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xuất bản bài viết có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ArticleDto>.ErrorResponse(
                "Đã xảy ra lỗi khi xuất bản bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Xóa bài viết
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteArticle(Guid id)
    {
        try
        {
            var result = await _articleService.DeleteArticleAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa bài viết thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài viết có ID: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi xóa bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Lấy chi tiết bài viết đầy đủ (Article + Comments + Likes)
    /// </summary>
    [HttpGet("by-slug/{slug}/full-detail")]
    [ProducesResponseType(typeof(ApiResponse<ArticleFullDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ArticleFullDetailDto>>> GetArticleFullDetail(string slug)
    {
        try
        {
            var userId = GetCurrentUserId();
            Guid? currentUserId = userId == Guid.Empty ? null : userId;

            var result = await _articleService.GetFullDetailBySlugAsync(slug, currentUserId);
            
            if (result == null)
            {
                return NotFound(ApiResponse<ArticleFullDetailDto>.NotFoundResponse($"Không tìm thấy bài viết: {slug}"));
            }

            return Ok(ApiResponse<ArticleFullDetailDto>.SuccessResponse(
                result,
                "Lấy chi tiết bài viết thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy full detail bài viết: {Slug}", slug);
            return StatusCode(500, ApiResponse<ArticleFullDetailDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy bài viết",
                500
            ));
        }
    }

    // TODO: Implement these methods in ArticleService
    /*
    /// <summary>
    /// Tăng view count
    /// </summary>
    [HttpPost("{id:guid}/view")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> IncrementView(Guid id)
    {
        try
        {
            await _articleService.IncrementViewCountAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(
                true,
                "Tăng lượt xem thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tăng view count cho bài viết: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi tăng lượt xem",
                500
            ));
        }
    }

    /// <summary>
    /// Like bài viết (yêu cầu đăng nhập)
    /// </summary>
    [HttpPost("{id:guid}/like")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> LikeArticle(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(
                    "Bạn cần đăng nhập để thích bài viết",
                    401
                ));
            }

            await _articleService.LikeArticleAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                true,
                "Thích bài viết thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi like bài viết: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi thích bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Unlike bài viết (yêu cầu đăng nhập)
    /// </summary>
    [HttpDelete("{id:guid}/like")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> UnlikeArticle(Guid id)
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

            await _articleService.UnlikeArticleAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                true,
                "Bỏ thích bài viết thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy bài viết có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi unlike bài viết: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi bỏ thích bài viết",
                500
            ));
        }
    }

    /// <summary>
    /// Kiểm tra user đã like bài viết chưa
    /// </summary>
    [HttpGet("{id:guid}/liked")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> CheckUserLiked(Guid id)
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

            var hasLiked = await _articleService.HasUserLikedAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(
                hasLiked,
                hasLiked ? "Đã thích" : "Chưa thích"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra like status: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi",
                500
            ));
        }
    }
    */
}
