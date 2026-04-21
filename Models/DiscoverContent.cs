using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("discover_content")]
public class DiscoverContent
{
    [Key]
    public uint ContentId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    [Required]
    public string BodyAr { get; set; } = null!;

    [Required]
    public string BodyEn { get; set; } = null!;

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
