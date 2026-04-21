using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("landmark_media")]
public class LandmarkMedia
{
    [Key]
    public uint MediaId { get; set; }

    [Required]
    public uint LandmarkId { get; set; }

    [Required]
    [MaxLength(500)]
    public string MediaUrl { get; set; } = null!;

    [Required]
    public MediaType MediaType { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(LandmarkId))]
    public Landmark Landmark { get; set; } = null!;
}
