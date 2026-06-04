namespace Had360back.DTOs.DiscoverContent;

/// <summary>
/// DTO كامل يُستخدم في صفحة تفاصيل محتوى الاكتشاف.
/// يحتوي على جميع الحقول بما فيها المحتوى النصي الكامل.
/// </summary>
public class DiscoverContentDetailsDto
{
    public int ContentId { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string BodyAr { get; set; } = null!;
    public string BodyEn { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
}
