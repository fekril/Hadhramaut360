using Had360back.Models;

namespace Had360back.DTOs.CommunityPosts;

/// <summary>
/// DTO لميديا المنشور. يُستخدم داخل CommunityPostResponseDto.
/// </summary>
public class CommunityPostMediaResponseDto
{
    public int MediaId { get; set; }
    public int PostId { get; set; }
    public string MediaUrl { get; set; } = null!;
    public MediaType MediaType { get; set; }
}
