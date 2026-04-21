using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Had360back.Models;

[Table("categories")]
public class Category
{
    [Key]
    public uint CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryNameAr { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string CategoryNameEn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<LandmarkCategory> LandmarkCategories { get; set; } = new List<LandmarkCategory>();
}
