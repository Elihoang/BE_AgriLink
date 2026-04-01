using AgriLink_DH.Core.Services;
using AgriLink_DH.Core.Interfaces;
using AgriLink_DH.Share.Common;
using AgriLink_DH.Share.DTOs.Product;
using Microsoft.AspNetCore.Mvc;

namespace AgriLink_DH.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ProductService productService,
        ICloudinaryService cloudinaryService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    /// <summary>
    /// Upload ảnh cho sản phẩm cụ thể. Trả về URL của ảnh đã upload.
    /// </summary>
    [HttpPost("upload-image/{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<string>>> UploadProductImage(Guid id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("Vui lòng chọn file ảnh."));

            // 1. Kiểm tra sản phẩm tồn tại
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Không tìm thấy sản phẩm."));
            }

            // 2. Upload lên Cloudinary (folder 'products')
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "products");

            // 3. Cập nhật vào DB
            await _productService.UpdateProductImageAsync(id, imageUrl);

            return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Upload ảnh sản phẩm thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi upload ảnh sản phẩm {ProductId}", id);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Lỗi hệ thống khi upload ảnh sản phẩm."));
        }
    }

    
    /// Lấy danh sách tất cả sản phẩm
    
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(
                products,
                "Lấy danh sách sản phẩm thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm");
            return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy danh sách sản phẩm",
                500
            ));
        }
    }

    
    /// Lấy sản phẩm theo ID

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.NotFoundResponse(
                    $"Không tìm thấy sản phẩm với ID: {id}"
                ));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(
                product,
                "Lấy thông tin sản phẩm thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy sản phẩm có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin sản phẩm",
                500
            ));
        }
    }

    
    /// Lấy sản phẩm theo Code
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductByCode(string code)
    {
        try
        {
            var product = await _productService.GetProductByCodeAsync(code);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.NotFoundResponse(
                    $"Không tìm thấy sản phẩm với mã: {code}"
                ));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(
                product,
                "Lấy thông tin sản phẩm thành công"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy sản phẩm có mã: {Code}", code);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse(
                "Đã xảy ra lỗi khi lấy thông tin sản phẩm",
                500
            ));
        }
    }

    /// <summary>
    /// Tạo mới sản phẩm (Hỗ trợ upload ảnh trực tiếp qua Multipart Form)
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromForm] CreateProductDto dto, IFormFile? file)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            // Nếu có file kèm theo khi tạo mới, upload lên Cloudinary ngay
            if (file != null && file.Length > 0)
            {
                dto.ImageUrl = await _cloudinaryService.UploadImageAsync(file, "products");
            }

            var product = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.Id },
                ApiResponse<ProductDto>.CreatedResponse(
                    product,
                    "Tạo sản phẩm mới thành công"
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi tạo sản phẩm: {Message}", ex.Message);
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo sản phẩm mới");
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse(
                "Đã xảy ra lỗi khi tạo sản phẩm",
                500
            ));
        }
    }

    /// <summary>
    /// Cập nhật thông tin sản phẩm (Hỗ trợ upload ảnh mới trực tiếp qua Multipart Form)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(
        Guid id,
        [FromForm] UpdateProductDto dto,
        IFormFile? file)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse(
                    "Dữ liệu không hợp lệ",
                    400,
                    ModelState
                ));
            }

            // Nếu người dùng chọn file ảnh mới khi Update, upload lên Cloudinary trước
            if (file != null && file.Length > 0)
            {
                dto.ImageUrl = await _cloudinaryService.UploadImageAsync(file, "products");
            }

            var product = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.SuccessResponse(
                product,
                "Cập nhật sản phẩm thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy sản phẩm có ID: {Id}", id);
            return NotFound(ApiResponse<ProductDto>.NotFoundResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Lỗi khi cập nhật sản phẩm: {Message}", ex.Message);
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm có ID: {Id}", id);
            return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse(
                "Đã xảy ra lỗi khi cập nhật sản phẩm",
                500
            ));
        }
    }

    
    /// Xóa sản phẩm
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(Guid id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(
                result,
                "Xóa sản phẩm thành công"
            ));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Không tìm thấy sản phẩm có ID: {Id}", id);
            return NotFound(ApiResponse<bool>.NotFoundResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa sản phẩm có ID: {Id}", id);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                "Đã xảy ra lỗi khi xóa sản phẩm",
                500
            ));
        }
    }
}
