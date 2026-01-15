using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace wedeli.Models.Domain;

[Table("ratings")]
[Index("CustomerId", Name = "customer_id")]
[Index("DriverId", Name = "driver_id")]
[Index("OrderId", Name = "order_id")]
public partial class Rating
{
    [Key]
    [Column("rating_id")]
    public int RatingId { get; set; }

    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("driver_id")]
    public int? DriverId { get; set; }

    [Column("rating_score")]
    public int? RatingScore { get; set; }

    [Column("review_text", TypeName = "text")]
    public string? ReviewText { get; set; }

    [Column("created_at", TypeName = "timestamp")]
    public DateTime? CreatedAt { get; set; }

    [NotMapped]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("DriverId")]
    [InverseProperty("Ratings")]
    public virtual Driver? Driver { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Ratings")]
    public virtual Order Order { get; set; } = null!;
}
