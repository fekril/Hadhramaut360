using System;
using System.Collections.Generic;
using Had360back.DTOs.CommunityPosts;

namespace Had360back.DTOs.CommunityPostReports;

/// <summary>
/// بيانات المنشور المُبلَّغ عنه. يُستخدم في صفحة بلاغات الأدمن (الجانب الأيسر).
/// يتضمن بيانات المنشور كاملة مع صاحبه وميديا.
/// </summary>
public class ReportedPostDto
{
    public int PostId { get; set; }
    public string ContentText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public CommunityPostUserDto User { get; set; } = null!;
    public List<CommunityPostMediaResponseDto> Media { get; set; } = new();
}
