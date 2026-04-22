using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("landmark_media")]
public class LandmarkMedia
{
    [Key]
    public int MediaId { get; set; }

    [Required]
    public int LandmarkId { get; set; }

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
