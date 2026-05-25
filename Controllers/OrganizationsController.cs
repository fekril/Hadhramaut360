using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Dtos.Organizations;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// متحكم لإدارة الملف الشخصي للمؤسسة الحالية.
/// </summary>
[ApiController]
[Route(ApiRoutes.Organizations.Base)]
[Authorize(Policy = AuthorizationPolicies.OrganizationOnly)]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    /// <summary>
    /// جلب تفاصيل الملف الشخصي للمؤسسة الحالية المسجل دخولها.
    /// </summary>
    /// <returns>بيانات الملف الشخصي للمؤسسة</returns>
    [HttpGet(ApiRoutes.Organizations.Me)]
    public async Task<IActionResult> GetMe()
    {
        var result = await _organizationService.GetMeAsync();
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// تحديث تفاصيل الملف الشخصي للمؤسسة الحالية مع إمكانية تحديث الشعار ورقم الهاتف.
    /// </summary>
    /// <param name="dto">بيانات التحديث والملفات المرفوعة</param>
    /// <returns>بيانات الملف الشخصي المحدثة للمؤسسة</returns>
    [HttpPut(ApiRoutes.Organizations.Me)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateMe([FromForm] UpdateOrganizationProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _organizationService.UpdateMeAsync(dto);
        return StatusCode(result.StatusCode, result);
    }
}
