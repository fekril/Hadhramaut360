using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("favorites")]
public class Favorite
{
    [Required]
    public uint UserId { get; set; }

    [Required]
    public uint LandmarkId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(LandmarkId))]
    public Landmark Landmark { get; set; } = null!;
}
