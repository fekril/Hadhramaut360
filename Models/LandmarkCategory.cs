using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("landmark_categories")]
public class LandmarkCategory
{
    [Required]
    public uint LandmarkId { get; set; }

    [Required]
    public uint CategoryId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(LandmarkId))]
    public Landmark Landmark { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}
