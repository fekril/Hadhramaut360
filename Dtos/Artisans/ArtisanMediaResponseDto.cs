using Had360back.Models;

namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO يُستخدم لإرجاع بيانات ميديا الحرفي بعد عمليات الإضافة أو التعديل أو في قائمة الميديا.
/// </summary>
public class ArtisanMediaResponseDto
{
    public int MediaId { get; set; }
    public int ArtisanId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType MediaType { get; set; }
    public bool IsCover { get; set; }
}
