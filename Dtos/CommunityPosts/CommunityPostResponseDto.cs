using System;
using System.Collections.Generic;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO المنشور المجتمعي المستخدم في الفيد العام وعند الإنشاء/التعديل.
/// يتضمن بيانات صاحب المنشور والميديا المرتبطة به.
/// </summary>
public class CommunityPostResponseDto
{
    public int PostId { get; set; }
    public string ContentText { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public CommunityPostUserDto User { get; set; } = null!;
    public List<CommunityPostMediaResponseDto> Media { get; set; } = new();
}
