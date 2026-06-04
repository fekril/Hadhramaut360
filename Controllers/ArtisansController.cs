using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Artisans;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة جميع واجهات API الخاصة بالحرفيين.
/// يوفر عمليات عرض الحرفيين، عرض التفاصيل، الإضافة، التعديل، الحذف، وإدارة ميديا الحرفيين.
/// </summary>
[ApiController]
[Route(ApiRoutes.Artisans.Base)]
public class ArtisansController : ControllerBase
{
    private readonly IArtisanService _artisanService;

    public ArtisansController(IArtisanService artisanService)
    {
        _artisanService = artisanService;
    }

    // ─── Public Endpoints ────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب قائمة الحرفيين مع دعم البحث والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">معاملات البحث والصفحات: Search، PageNumber، PageSize.</param>
    /// <returns>يرجع ApiResponse يحتوي على قائمة مختصرة من ArtisanResponseDto مع بيانات الصفحات.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// كل حرفي يحتوي على صورة الغلاف المأخوذة من ArtisanMedia حيث IsCover = true.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ArtisanResponseDto>>>> GetAll(
        [FromQuery] ArtisanQueryDto query)
    {
        var result = await _artisanService.GetAllAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب التفاصيل الكاملة لحرفي محدد حسب رقمه.
    /// </summary>
    /// <param name="id">رقم الحرفي المراد عرض تفاصيله.</param>
    /// <returns>يرجع ApiResponse يحتوي على ArtisanDetailsDto إذا تم العثور على الحرفي، أو خطأ 404.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    /// </remarks>
    [HttpGet(ApiRoutes.Artisans.ById)]
    public async Task<ActionResult<ApiResponse<ArtisanDetailsDto>>> GetById(int id)
    {
        var result = await _artisanService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Management Endpoints ────────────────────────────────────────────────────

    /// <summary>
    /// ينشئ حرفيًا جديدًا مع إمكانية رفع ملفات ميديا مرتبطة به.
    /// أول ملف يُرفع يُعيَّن صورة غلاف.
    /// </summary>
    /// <param name="dto">بيانات الحرفي الجديد بالإضافة إلى الملفات المراد رفعها (multipart/form-data).</param>
    /// <returns>يرجع ApiResponse يحتوي على تفاصيل الحرفي الجديد مع الميديا المرتبطة.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// </remarks>
    [HttpPost(ApiRoutes.Artisans.WithMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ArtisanDetailsDto>>> CreateWithMedia(
        [FromForm] CreateArtisanWithMediaDto dto)
    {
        var result = await _artisanService.CreateWithMediaAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحدث البيانات النصية لحرفي موجود.
    /// لا يحدث الميديا — إدارة الميديا لها endpoints منفصلة.
    /// </summary>
    /// <param name="id">رقم الحرفي المراد تعديله.</param>
    /// <param name="dto">البيانات النصية الجديدة للحرفي.</param>
    /// <returns>يرجع ApiResponse يحتوي على تفاصيل الحرفي بعد التعديل، أو خطأ 404.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// Body Type: JSON
    /// </remarks>
    [HttpPut(ApiRoutes.Artisans.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<ArtisanDetailsDto>>> Update(
        int id, [FromBody] UpdateArtisanDto dto)
    {
        var result = await _artisanService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف حرفيًا موجودًا وجميع ملفات الميديا المرتبطة به.
    /// </summary>
    /// <param name="id">رقم الحرفي المراد حذفه.</param>
    /// <returns>يرجع ApiResponse بقيمة true عند نجاح الحذف، أو خطأ 404.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy
    /// </remarks>
    [HttpDelete(ApiRoutes.Artisans.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _artisanService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Media Management Endpoints ──────────────────────────────────────────────

    /// <summary>
    /// يضيف ملف ميديا واحدًا لحرفي موجود.
    /// إذا لم يكن للحرفي صورة غلاف، يُعيَّن الملف الجديد صورة غلاف.
    /// </summary>
    /// <param name="id">رقم الحرفي الذي سيتم إضافة الميديا له.</param>
    /// <param name="dto">الملف المراد رفعه.</param>
    /// <returns>يرجع ApiResponse يحتوي على ArtisanMediaResponseDto بعد الرفع برمز 201.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// </remarks>
    [HttpPost(ApiRoutes.Artisans.AddMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ArtisanMediaResponseDto>>> AddMedia(
        int id, [FromForm] AddArtisanMediaDto dto)
    {
        var result = await _artisanService.AddMediaAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يستبدل ملف ميديا موجود بملف جديد مع الحفاظ على قيمة IsCover.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد تحديثه.</param>
    /// <param name="dto">الملف الجديد.</param>
    /// <returns>يرجع ApiResponse يحتوي على ArtisanMediaResponseDto بعد التحديث.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// Content-Type: multipart/form-data
    /// </remarks>
    [HttpPut(ApiRoutes.Artisans.UpdateMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ArtisanMediaResponseDto>>> UpdateMedia(
        int mediaId, [FromForm] UpdateArtisanMediaDto dto)
    {
        var result = await _artisanService.UpdateMediaAsync(mediaId, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف ملف ميديا من قاعدة البيانات ومن التخزين.
    /// إذا كانت الميديا المحذوفة هي صورة الغلاف، يُرقَّى ملف ميديا آخر ليصبح الغلاف.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد حذفه.</param>
    /// <returns>يرجع ApiResponse بقيمة true عند نجاح الحذف، أو خطأ 404.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy
    /// </remarks>
    [HttpDelete(ApiRoutes.Artisans.DeleteMedia)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int mediaId)
    {
        var result = await _artisanService.DeleteMediaAsync(mediaId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يُعيِّن ملف ميديا محدد كصورة غلاف للحرفي.
    /// يُعيد تعيين IsCover = false لجميع ميديا الحرفي، ثم يُعيِّن الميديا المختارة كغلاف.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد تعيينه كغلاف.</param>
    /// <returns>يرجع ApiResponse يحتوي على ArtisanMediaResponseDto بعد التحديث.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// </remarks>
    [HttpPut(ApiRoutes.Artisans.SetCover)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<ArtisanMediaResponseDto>>> SetCover(int mediaId)
    {
        var result = await _artisanService.SetCoverAsync(mediaId);
        return StatusCode(result.StatusCode, result);
    }
}
