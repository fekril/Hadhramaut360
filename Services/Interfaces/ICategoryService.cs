using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Categories;

namespace Had360back.Services.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<List<CategoryResponseDto>>> GetAllAsync();
    Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto);
    Task<ApiResponse<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
