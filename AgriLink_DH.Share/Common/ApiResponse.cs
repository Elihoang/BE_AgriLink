namespace AgriLink_DH.Share.Common;

/// <summary>
/// Wrapper class cho API Response
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Thành công")
    {
        return new ApiResponse<T>
        {
            StatusCode = 200,
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> CreatedResponse(T data, string message = "Tạo mới thành công")
    {
        return new ApiResponse<T>
        {
            StatusCode = 201,
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, object? errors = null)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    public static ApiResponse<T> NotFoundResponse(string message = "Không tìm thấy dữ liệu")
    {
        return new ApiResponse<T>
        {
            StatusCode = 404,
            Success = false,
            Message = message
        };
    }
}
