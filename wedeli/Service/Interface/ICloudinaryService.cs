using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace wedeli.Service.Interface
{
    /// <summary>
    /// Cloudinary image service interface
    /// </summary>
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "orders");
        Task<string> UploadImageFromBase64Async(string base64Image, string fileName, string folder = "orders");
        Task<bool> DeleteImageAsync(string publicId);
        Task<IEnumerable<string>> UploadMultipleImagesAsync(IEnumerable<byte[]> images, string folder = "orders");
    }
}
