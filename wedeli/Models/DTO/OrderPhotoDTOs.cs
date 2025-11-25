namespace wedeli.Models.DTO
{
    /// <summary>
    /// Request DTO for uploading a photo
    /// </summary>
    public class UploadPhotoRequestDto
    {
        public int OrderId { get; set; }
        public string PhotoType { get; set; } = null!; // before_delivery, after_delivery, parcel_condition, signature, damage_proof
        public IFormFile Photo { get; set; } = null!;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response DTO after photo upload
    /// </summary>
    public class PhotoUploadResponseDto
    {
        public int PhotoId { get; set; }
        public int OrderId { get; set; }
        public string PhotoType { get; set; } = null!;
        public string PhotoUrl { get; set; } = null!;
        public string? PublicId { get; set; }
        public int? FileSizeKb { get; set; }
        public int? UploadedBy { get; set; }
        public string? UploaderName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Message { get; set; } = "Photo uploaded successfully";
    }

    /// <summary>
    /// Response DTO for getting order photos
    /// </summary>
    public class OrderPhotoDto
    {
        public int PhotoId { get; set; }
        public int OrderId { get; set; }
        public string PhotoType { get; set; } = null!;
        public string PhotoUrl { get; set; } = null!;
        public string? FileName { get; set; }
        public int? FileSizeKb { get; set; }
        public int? UploadedBy { get; set; }
        public string? UploaderName { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for batch photo upload
    /// </summary>
    public class BatchUploadPhotosRequestDto
    {
        public int OrderId { get; set; }
        public string PhotoType { get; set; } = null!;
        public List<IFormFile> Photos { get; set; } = new();
    }

    /// <summary>
    /// Response DTO for batch upload
    /// </summary>
    public class BatchUploadResponseDto
    {
        public int TotalUploaded { get; set; }
        public int TotalFailed { get; set; }
        public List<PhotoUploadResponseDto> UploadedPhotos { get; set; } = new();
        public List<string> ErrorMessages { get; set; } = new();
    }
}