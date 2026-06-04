using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.DiscoverContent;

namespace Had360back.Services.Interfaces;

public interface IDiscoverContentService
{
    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف مع دعم البحث والتقسيم إلى صفحات.
    /// </summary>
    Task<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>> GetAllAsync(DiscoverContentQueryDto query);

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف المرتبطة بقسم معين (ثقافة، أكل، ألعاب...) بناءً على قائمة الكلمات المفتاحية.
    /// يدعم البحث الإضافي والتقسيم إلى صفحات.
    /// </summary>
    Task<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>> GetBySectionAsync(
        DiscoverContentQueryDto query,
        IReadOnlyList<string> keywords);

    /// <summary>
    /// يجلب تفاصيل عنصر محتوى اكتشاف واحد حسب رقمه.
    /// </summary>
    Task<ApiResponse<DiscoverContentDetailsDto>> GetByIdAsync(int id);

    /// <summary>
    /// ينشئ عنصر محتوى اكتشاف جديدًا.
    /// </summary>
    Task<ApiResponse<DiscoverContentDetailsDto>> CreateAsync(CreateDiscoverContentDto dto);

    /// <summary>
    /// يحدث عنصر محتوى اكتشاف موجودًا.
    /// </summary>
    Task<ApiResponse<DiscoverContentDetailsDto>> UpdateAsync(int id, UpdateDiscoverContentDto dto);

    /// <summary>
    /// يحذف عنصر محتوى اكتشاف موجودًا.
    /// </summary>
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
