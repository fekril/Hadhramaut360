using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("favorites")]
public class Favorite
{
    [Key]
    public int Id { get; set; } 
    [Required]
    public int UserId { get; set; }

    [Required]
    public int LandmarkId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(LandmarkId))]
    public Landmark Landmark { get; set; } = null!;
}
