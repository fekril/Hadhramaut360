using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.Categories;

public class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string CategoryNameAr { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string CategoryNameEn { get; set; } = null!;
}
