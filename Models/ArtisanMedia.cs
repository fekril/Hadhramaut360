using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("artisan_media")]
public class ArtisanMedia
{
    [Key]
    public int MediaId { get; set; }

    [Required]
    public int ArtisanId { get; set; }

    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = null!;

    [Required]
    public MediaType MediaType { get; set; }

    public bool IsCover { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ArtisanId))]
    public Artisan Artisan { get; set; } = null!;
}
