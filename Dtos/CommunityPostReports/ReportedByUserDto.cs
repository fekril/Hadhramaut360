namespace Had360back.DTOs.CommunityPostReports;

/// <summary>
/// بيانات المستخدم الذي أرسل البلاغ. يُستخدم في صفحة بلاغات الأدمن.
/// FullName هو الاسم المعروض للمستخدم.
/// </summary>
public class ReportedByUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
