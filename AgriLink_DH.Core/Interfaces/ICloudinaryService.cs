using Microsoft.AspNetCore.Http;

namespace AgriLink_DH.Core.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload image to Cloudinary
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="folder">Target folder in Cloudinary (e.g., "products", "articles")</param>
    /// <returns>Secure URL of the uploaded image</returns>
    Task<string> UploadImageAsync(IFormFile file, string folder);

    /// <summary>
    /// Delete image from Cloudinary
    /// </summary>
    /// <param name="publicId">Public ID of the image</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteImageAsync(string publicId);
}
