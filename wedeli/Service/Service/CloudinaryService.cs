using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wedeli.Service.Interface;

namespace wedeli.Service.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _defaultFolder;

        public CloudinaryService(IConfiguration config)
        {
            var settings = config.GetSection("Cloudinary");
            _defaultFolder = settings["Folder"] ?? "uploads";

            Account account = new Account(
                settings["CloudName"],
                settings["ApiKey"],
                settings["ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(byte[] imageData, string fileName, string folder = "orders")
        {
            using var stream = new MemoryStream(imageData);

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = folder ?? _defaultFolder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString();
        }

        public async Task<string> UploadImageFromBase64Async(string base64Image, string fileName, string folder = "orders")
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, $"data:image/jpeg;base64,{base64Image}"),
                Folder = folder ?? _defaultFolder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult?.SecureUrl?.ToString();
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var delParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(delParams);
            return result.Result == "ok";
        }

        public async Task<IEnumerable<string>> UploadMultipleImagesAsync(IEnumerable<byte[]> images, string folder = "orders")
        {
            var urls = new List<string>();

            foreach (var img in images)
            {
                var url = await UploadImageAsync(img, Guid.NewGuid().ToString(), folder);
                if (url != null) urls.Add(url);
            }

            return urls;
        }
    }
}
