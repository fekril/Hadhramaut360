namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// بيانات المستخدم المضمّنة في استجابة المنشور.
/// تُستخدم لعرض بيانات صاحب المنشور في الفيد والتفاصيل.
/// </summary>
public class CommunityPostUserDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
