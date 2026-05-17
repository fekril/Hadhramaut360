using System;

namespace Had360back.DTOs.Categories;

public class CategoryResponseDto
{
    public int CategoryId { get; set; }
    public string CategoryNameAr { get; set; } = null!;
    public string CategoryNameEn { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
