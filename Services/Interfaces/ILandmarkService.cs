using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Landmarks;

namespace Had360back.Services.Interfaces;

public interface ILandmarkService
{
    Task<ApiResponse<PaginatedResponse<LandmarkResponseDto>>> GetAllAsync(LandmarkQueryDto query);
    Task<ApiResponse<LandmarkDetailsDto>> GetByIdAsync(int id);
    Task<ApiResponse<LandmarkDetailsDto>> CreateAsync(CreateLandmarkDto dto);
    Task<ApiResponse<LandmarkDetailsDto>> CreateWithMediaAsync(CreateLandmarkWithMediaDto dto);
    Task<ApiResponse<LandmarkDetailsDto>> UpdateAsync(int id, UpdateLandmarkDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    
    Task<ApiResponse<LandmarkMediaResponseDto>> AddMediaAsync(int landmarkId, AddLandmarkMediaDto dto);
    Task<ApiResponse<LandmarkMediaResponseDto>> UpdateMediaAsync(int mediaId, UpdateLandmarkMediaDto dto);
    Task<ApiResponse<bool>> DeleteMediaAsync(int mediaId);
}
