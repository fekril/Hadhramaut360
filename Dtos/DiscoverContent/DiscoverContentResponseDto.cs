namespace Had360back.DTOs.DiscoverContent;

/// <summary>
/// DTO مختصر يُستخدم في قوائم محتوى الاكتشاف (عرض البطاقات).
/// لا يحتوي على المحتوى النصي الكامل للحفاظ على خفة الاستجابة.
/// </summary>
public class DiscoverContentResponseDto
{
    public int ContentId { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
}
