namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// بيانات المستخدم المضمّنة في استجابة المنشور.
/// تُستخدم لعرض بيانات صاحب المنشور في الفيد والتفاصيل.
/// FullName هو الاسم المعروض للمستخدم — مختلف عن UserName الداخلي في Identity.
/// </summary>
public class CommunityPostUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
