namespace Had360back.DTOs.CommunityPostReports;

/// <summary>
/// بيانات المستخدم الذي أرسل البلاغ. يُستخدم في صفحة بلاغات الأدمن.
/// </summary>
public class ReportedByUserDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
