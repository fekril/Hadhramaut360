using System;

namespace Had360back.DTOs.CommunityPostReports;

/// <summary>
/// DTO استجابة البلاغ الكامل للأدمن.
/// يتضمن بيانات البلاغ، المُبلِّغ، والمنشور المُبلَّغ عنه كاملًا.
/// يُستخدم في صفحة بلاغات الأدمن.
/// </summary>
public class CommunityPostReportResponseDto
{
    /// <summary>رقم البلاغ.</summary>
    public int ReportId { get; set; }

    /// <summary>سبب البلاغ.</summary>
    public string Reason { get; set; } = null!;

    /// <summary>تاريخ إرسال البلاغ.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>بيانات المستخدم الذي أرسل البلاغ.</summary>
    public ReportedByUserDto ReportedBy { get; set; } = null!;

    /// <summary>بيانات المنشور المُبلَّغ عنه مع صاحبه وميديا.</summary>
    public ReportedPostDto ReportedPost { get; set; } = null!;
}
