using System;
using System.Collections.Generic;
using Had360back.DTOs.Categories;

namespace Had360back.DTOs.Landmarks;

public class LandmarkDetailsDto
{
    public int LandmarkId { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string DescriptionAr { get; set; } = null!;
    public string DescriptionEn { get; set; } = null!;
    public string? LocationTextAr { get; set; }
    public string? LocationTextEn { get; set; }
    public string MapUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<CategoryResponseDto> Categories { get; set; } = new();
    public List<LandmarkMediaResponseDto> Media { get; set; } = new();

    /// <summary>
    /// محسوب من جدول المفضلات بناءً على المستخدم الحالي. لا يُحفظ في قاعدة البيانات.
    /// </summary>
    public bool IsFavorite { get; set; }
}
