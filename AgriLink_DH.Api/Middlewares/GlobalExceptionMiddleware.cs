using AgriLink_DH.Share.Common;
using System.Net;
using System.Text.Json;

namespace AgriLink_DH.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Cho phép request đi tiếp qua các Middleware khác và Controllers
            await _next(context);
        }
        catch (Exception ex)
        {
            // Bắt và xử lý mọi Exception văng ra trong quá trình xử lý request
            _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "Đã xảy ra lỗi hệ thống, vui lòng thử lại sau";

        // Tùy biến mã HTTP và Message theo loại Exception
        switch (exception)
        {
            case InvalidOperationException ex:
                statusCode = (int)HttpStatusCode.BadRequest; // 400
                message = ex.Message;
                break;
            case KeyNotFoundException ex:
                statusCode = (int)HttpStatusCode.NotFound; // 404
                message = ex.Message;
                break;
            case UnauthorizedAccessException ex:
                statusCode = (int)HttpStatusCode.Forbidden; // 403
                message = ex.Message;
                break;
            case ArgumentException ex:
                statusCode = (int)HttpStatusCode.BadRequest; // 400
                message = ex.Message;
                break;
        }

        context.Response.StatusCode = statusCode;

        // Trả về theo chuẩn ApiResponse chung của dự án
        var response = ApiResponse<object>.ErrorResponse(message, statusCode);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(jsonResponse);
    }
}
