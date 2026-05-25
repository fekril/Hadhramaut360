using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Dtos.Organizations;

namespace Had360back.Services.Interfaces;

/// <summary>
/// واجهة لإدارة الملف الشخصي للمؤسسة.
/// </summary>
public interface IOrganizationService
{
    /// <summary>
    /// جلب بيانات الملف الشخصي للمؤسسة الحالية.
    /// </summary>
    /// <returns>بيانات الملف الشخصي للمؤسسة</returns>
    Task<ApiResponse<OrganizationProfileDto>> GetMeAsync();

    /// <summary>
    /// تحديث بيانات الملف الشخصي للمؤسسة الحالية.
    /// </summary>
    /// <param name="dto">بيانات التحديث</param>
    /// <returns>بيانات الملف الشخصي للمؤسسة بعد التحديث</returns>
    Task<ApiResponse<OrganizationProfileDto>> UpdateMeAsync(UpdateOrganizationProfileDto dto);
}
