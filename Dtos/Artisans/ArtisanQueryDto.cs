using System.ComponentModel.DataAnnotations;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// معاملات الاستعلام للحصول على قائمة الحرفيين مع دعم البحث والتقسيم إلى صفحات.
/// </summary>
public class ArtisanQueryDto
{
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
