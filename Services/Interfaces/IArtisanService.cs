using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Artisans;

namespace Had360back.Services.Interfaces;

/// <summary>
/// واجهة سيرفس إدارة الحرفيين وميديا الحرفيين.
/// </summary>
public interface IArtisanService
{
    // ─── Public Endpoints ────────────────────────────────────────────────────────

    Task<ApiResponse<PaginatedResponse<ArtisanResponseDto>>> GetAllAsync(ArtisanQueryDto query);
    Task<ApiResponse<ArtisanDetailsDto>> GetByIdAsync(int id);

    // ─── Management Endpoints ────────────────────────────────────────────────────

    Task<ApiResponse<ArtisanDetailsDto>> CreateWithMediaAsync(CreateArtisanWithMediaDto dto);
    Task<ApiResponse<ArtisanDetailsDto>> UpdateAsync(int id, UpdateArtisanDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);

    // ─── Media Management Endpoints ──────────────────────────────────────────────

    Task<ApiResponse<ArtisanMediaResponseDto>> AddMediaAsync(int artisanId, AddArtisanMediaDto dto);
    Task<ApiResponse<ArtisanMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateArtisanMediaDto dto);
    Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId);
    Task<ApiResponse<ArtisanMediaResponseDto>> SetCoverAsync(int mediaId);
}
