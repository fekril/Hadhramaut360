using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("landmarks")]
public class Landmark
{
    [Key]
    public int LandmarkId { get; set; }

    [Required]
    [MaxLength(200)]
    public string TitleAr { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TitleEn { get; set; } = null!;

    [Required]
    public string DescriptionAr { get; set; } = null!;

    [Required]
    public string DescriptionEn { get; set; } = null!;

    [MaxLength(255)]
    public string? LocationTextAr { get; set; }

    [MaxLength(255)]
    public string? LocationTextEn { get; set; }

    [Required]
    [MaxLength(1000)]
    public string MapUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<LandmarkCategory> LandmarkCategories { get; set; } = new List<LandmarkCategory>();
    public ICollection<LandmarkMedia> LandmarkMedia { get; set; } = new List<LandmarkMedia>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
