using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.CommunityPosts;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة جميع واجهات API الخاصة بمنشورات المجتمع.
/// يوفر عرض الفيد العام، إنشاء المنشورات، التعديل، الحذف، إدارة الميديا، والإبلاغ عن المنشورات.
/// </summary>
[ApiController]
[Route(ApiRoutes.CommunityPosts.Base)]
public class CommunityPostsController : ControllerBase
{
    private readonly ICommunityPostService _communityPostService;

    public CommunityPostsController(ICommunityPostService communityPostService)
    {
        _communityPostService = communityPostService;
    }

    // ─── Public Feed ──────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب الفيد العام للمنشورات المجتمعية مع التقسيم إلى صفحات والبحث الاختياري.
    /// </summary>
    /// <param name="query">معاملات الاستعلام: Search (اختياري)، PageNumber، PageSize.</param>
    /// <returns>قائمة منقّحة من CommunityPostResponseDto مرتبة من الأحدث إلى الأقدم.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// كل منشور يتضمن بيانات صاحبه وصور الميديا المرتبطة به.
    /// CreatedAt مُضمَّن لعرض الوقت النسبي في الفرونت إند.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CommunityPostResponseDto>>>> GetAll(
        [FromQuery] CommunityPostQueryDto query)
    {
        var result = await _communityPostService.GetAllAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Authenticated User Actions ───────────────────────────────────────────

    /// <summary>
    /// ينشئ منشورًا مجتمعيًا جديدًا مع إمكانية رفع صور.
    /// رقم المستخدم يُؤخذ تلقائيًا من JWT.
    /// </summary>
    /// <param name="dto">نص المنشور والملفات المراد رفعها (multipart/form-data). الحد الأقصى 5 صور.</param>
    /// <returns>بيانات المنشور المُنشأ برمز 201.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated
    /// Content-Type: multipart/form-data
    ///
    /// النشر النصي بدون صور مسموح به.
    /// يستخدم Transaction لضمان اتساق البيانات.
    /// </remarks>
    [Authorize]
    [HttpPost(ApiRoutes.CommunityPosts.WithMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CommunityPostResponseDto>>> CreateWithMedia(
        [FromForm] CreateCommunityPostWithMediaDto dto)
    {
        var result = await _communityPostService.CreateWithMediaAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحدّث نص المنشور المجتمعي.
    /// يُسمح لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم المنشور المراد تعديله.</param>
    /// <param name="dto">النص الجديد.</param>
    /// <returns>بيانات المنشور بعد التعديل، أو خطأ 404 / 403.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Authenticated — Post owner, Admin, or ContentManager
    /// Body Type: JSON
    /// </remarks>
    [Authorize]
    [HttpPut(ApiRoutes.CommunityPosts.ById)]
    public async Task<ActionResult<ApiResponse<CommunityPostResponseDto>>> Update(
        int id, [FromBody] UpdateCommunityPostDto dto)
    {
        var result = await _communityPostService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف منشورًا مجتمعيًا وجميع ميديا وبلاغاته.
    /// يُسمح لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم المنشور المراد حذفه.</param>
    /// <returns>true عند نجاح الحذف، أو خطأ 404 / 403.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Authenticated — Post owner, Admin, or ContentManager
    /// </remarks>
    [Authorize]
    [HttpDelete(ApiRoutes.CommunityPosts.ById)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _communityPostService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يُرسل بلاغًا على منشور مجتمعي.
    /// رقم المستخدم يُؤخذ من JWT.
    /// </summary>
    /// <param name="id">رقم المنشور المراد الإبلاغ عنه.</param>
    /// <param name="dto">سبب البلاغ.</param>
    /// <returns>بيانات البلاغ المُنشأ برمز 201، أو رسالة خطأ واضحة.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated
    ///
    /// لا يمكن الإبلاغ على نفس المنشور مرتين.
    /// لا يمكن الإبلاغ على منشورك الخاص.
    /// </remarks>
    [Authorize]
    [HttpPost(ApiRoutes.CommunityPosts.Report)]
    public async Task<ActionResult<ApiResponse<ReportResponseDto>>> Report(
        int id, [FromBody] CreateReportDto dto)
    {
        var result = await _communityPostService.ReportAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Media Management Endpoints ───────────────────────────────────────────

    /// <summary>
    /// يضيف ملف ميديا واحدًا لمنشور موجود.
    /// يُسمح لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="id">رقم المنشور.</param>
    /// <param name="dto">الملف المراد رفعه (multipart/form-data).</param>
    /// <returns>بيانات الميديا المضافة برمز 201، أو خطأ 404 / 403 / رسالة الحد الأقصى.</returns>
    /// <remarks>
    /// Method: POST
    /// Access: Authenticated — Post owner, Admin, or ContentManager
    /// Content-Type: multipart/form-data
    /// </remarks>
    [Authorize]
    [HttpPost(ApiRoutes.CommunityPosts.AddMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CommunityPostMediaResponseDto>>> AddMedia(
        int id, [FromForm] AddCommunityPostMediaDto dto)
    {
        var result = await _communityPostService.AddMediaAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يستبدل ملف ميديا موجود بملف جديد.
    /// يُسمح لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد تحديثه.</param>
    /// <param name="dto">الملف الجديد (multipart/form-data).</param>
    /// <returns>بيانات الميديا بعد التحديث، أو خطأ 404 / 403.</returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Authenticated — Post owner, Admin, or ContentManager
    /// Content-Type: multipart/form-data
    /// </remarks>
    [Authorize]
    [HttpPut(ApiRoutes.CommunityPosts.UpdateMedia)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CommunityPostMediaResponseDto>>> UpdateMedia(
        int mediaId, [FromForm] UpdateCommunityPostMediaDto dto)
    {
        var result = await _communityPostService.UpdateMediaAsync(mediaId, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف ملف ميديا من المنشور من قاعدة البيانات ومن التخزين الفعلي.
    /// يُسمح لصاحب المنشور أو الأدمن أو مدير المحتوى.
    /// </summary>
    /// <param name="mediaId">رقم سجل الميديا المراد حذفه.</param>
    /// <returns>true عند نجاح الحذف، أو خطأ 404 / 403.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Authenticated — Post owner, Admin, or ContentManager
    /// </remarks>
    [Authorize]
    [HttpDelete(ApiRoutes.CommunityPosts.DeleteMedia)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int mediaId)
    {
        var result = await _communityPostService.DeleteMediaAsync(mediaId);
        return StatusCode(result.StatusCode, result);
    }
}
