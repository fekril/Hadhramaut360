using System;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO استجابة البلاغ. يُرجع عند نجاح إرسال البلاغ.
/// </summary>
public class ReportResponseDto
{
    public int ReportId { get; set; }
    public int PostId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
