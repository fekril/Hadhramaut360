using System.Collections.Generic;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO كامل يُستخدم في صفحة تفاصيل الحرفي.
/// يحتوي على جميع الحقول المطلوبة في صفحة التفاصيل.
/// </summary>
public class ArtisanDetailsDto
{
    public int ArtisanId { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string DescriptionAr { get; set; } = null!;
    public string DescriptionEn { get; set; } = null!;
    public string? LocationTextAr { get; set; }
    public string? LocationTextEn { get; set; }
    public string? MapUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public List<ArtisanMediaResponseDto> Media { get; set; } = new();
}
