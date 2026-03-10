using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AgriLink_DH.Core.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName chưa được cấu hình.");
        var apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey chưa được cấu hình.");
        var apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret chưa được cấu hình.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    /// <summary>
    /// Upload file ảnh lên Cloudinary, trả về secure URL.
    /// </summary>
    public async Task<string> UploadImageAsync(IFormFile file, string folder = "agrilink/materials")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File ảnh không hợp lệ.");

        // Validate type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Chỉ chấp nhận ảnh định dạng JPG, PNG, WebP, GIF.");

        // Max 5MB
        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("Ảnh không được vượt quá 5MB.");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            // Tự động crop & resize nếu cần
            Transformation = new Transformation()
                .Width(800).Height(800)
                .Crop("limit")  // Không phóng to, chỉ thu nhỏ nếu lớn hơn 800x800
                .Quality("auto")
                .FetchFormat("auto"),
            UseFilenameAsDisplayName = true,
            UniqueFilename = true,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload lỗi: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    /// <summary>
    /// Xóa ảnh trên Cloudinary theo publicId (nếu cần cleanup).
    /// </summary>
    public async Task DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deleteParams);
    }
}
