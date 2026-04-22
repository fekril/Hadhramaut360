using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("landmark_categories")]
public class LandmarkCategory
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int LandmarkId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(LandmarkId))]
    public Landmark Landmark { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}
