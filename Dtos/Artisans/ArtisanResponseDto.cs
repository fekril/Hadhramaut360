namespace Had360back.DTOs.Artisans;

/// <summary>
/// DTO مختصر يُستخدم في قوائم الحرفيين (عرض البطاقات).
/// يحتوي فقط على المعرّف والاسمين وصورة الغلاف.
/// </summary>
public class ArtisanResponseDto
{
    public int ArtisanId { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string? CoverImageUrl { get; set; }
}
