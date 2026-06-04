using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.CommunityPosts;

namespace Had360back.Services.Interfaces;

/// <summary>
/// واجهة سيرفس إدارة المنشورات المجتمعية وميديا المنشورات.
/// </summary>
public interface ICommunityPostService
{
    // ─── Public Feed ──────────────────────────────────────────────────────────

    Task<ApiResponse<PaginatedResponse<CommunityPostResponseDto>>> GetAllAsync(CommunityPostQueryDto query);

    // ─── Authenticated User Actions ───────────────────────────────────────────

    Task<ApiResponse<CommunityPostResponseDto>> CreateWithMediaAsync(CreateCommunityPostWithMediaDto dto);
    Task<ApiResponse<CommunityPostResponseDto>> UpdateAsync(int id, UpdateCommunityPostDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);

    // ─── Media Management ────────────────────────────────────────────────────

    Task<ApiResponse<CommunityPostMediaResponseDto>> AddMediaAsync(int postId, AddCommunityPostMediaDto dto);
    Task<ApiResponse<CommunityPostMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateCommunityPostMediaDto dto);
    Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId);

    // ─── Reporting ────────────────────────────────────────────────────────────

    Task<ApiResponse<ReportResponseDto>> ReportAsync(int postId, CreateReportDto dto);
}
