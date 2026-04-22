using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("artisans")]
public class Artisan
{
    [Key]
    public int ArtisanId { get; set; }

    [Required]
    [MaxLength(150)]
    public string NameAr { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string NameEn { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = null!;

    [Required]
    public string DescriptionAr { get; set; } = null!;

    [Required]
    public string DescriptionEn { get; set; } = null!;

    [MaxLength(255)]
    public string? LocationTextAr { get; set; }

    [MaxLength(255)]
    public string? LocationTextEn { get; set; }

    [MaxLength(500)]
    public string? MapUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<ArtisanMedia> ArtisanMedia { get; set; } = new List<ArtisanMedia>();
}
