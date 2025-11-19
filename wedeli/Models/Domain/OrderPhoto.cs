using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("order_photos")]
[Index("OrderId", Name = "order_id")]
[Index("UploadedBy", Name = "uploaded_by")]
public partial class OrderPhoto
{
    [Key]
    [Column("photo_id")]
    public int PhotoId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("photo_type", TypeName = "enum('before_delivery','after_delivery','parcel_condition','signature','damage_proof')")]
    public string PhotoType { get; set; } = null!;

    /// <summary>
    /// URL từ cloud storage (AWS S3, Cloudinary, etc)
    /// </summary>
    [Column("photo_url")]
    [StringLength(500)]
    public string PhotoUrl { get; set; } = null!;

    [Column("file_name")]
    [StringLength(255)]
    public string? FileName { get; set; }

    [Column("file_size_kb")]
    public int? FileSizeKb { get; set; }

    [Column("uploaded_by")]
    public int? UploadedBy { get; set; }

    [Column("uploaded_at", TypeName = "timestamp")]
    public DateTime? UploadedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderPhotos")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("UploadedBy")]
    [InverseProperty("OrderPhotos")]
    public virtual User? UploadedByNavigation { get; set; }
}
