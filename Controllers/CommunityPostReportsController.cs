using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.CommunityPostReports;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن إدارة تقارير بلاغات المنشورات المجتمعية.
/// مخصص للأدمن ومدير المحتوى فقط.
/// يوفر عرض البلاغات، رفض البلاغ، وحذف المنشور المُبلَّغ عنه.
/// </summary>
[ApiController]
[Route(ApiRoutes.CommunityPostReports.Base)]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public class CommunityPostReportsController : ControllerBase
{
    private readonly ICommunityPostReportService _reportService;

    public CommunityPostReportsController(ICommunityPostReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// يجلب قائمة مقسّمة إلى صفحات من بلاغات المنشورات المجتمعية.
    /// مرتبة من الأحدث إلى الأقدم.
    /// </summary>
    /// <param name="query">معاملات التقسيم إلى صفحات: PageNumber، PageSize.</param>
    /// <returns>
    /// قائمة من CommunityPostReportResponseDto تتضمن بيانات البلاغ،
    /// المُبلِّغ، والمنشور المُبلَّغ عنه مع صاحبه وميديا.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Admin / ContentManager
    ///
    /// يُستخدم في صفحة بلاغات الأدمن.
    /// كل سجل يتضمن الجانب الأيسر (المنشور) والجانب الأيمن (المُبلِّغ والسبب).
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<CommunityPostReportResponseDto>>>> GetAll(
        [FromQuery] CommunityPostReportQueryDto query)
    {
        var result = await _reportService.GetAllReportsAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يرفض بلاغًا ويحذفه دون حذف المنشور.
    /// يُستخدم لزر "Dismiss Report" في صفحة بلاغات الأدمن.
    /// </summary>
    /// <param name="reportId">رقم البلاغ المراد رفضه.</param>
    /// <returns>true عند نجاح الحذف، أو خطأ 404 إذا لم يوجد البلاغ.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Admin / ContentManager
    ///
    /// يحذف سجل البلاغ فقط. المنشور يبقى كما هو.
    /// </remarks>
    [HttpDelete(ApiRoutes.CommunityPostReports.ByReportId)]
    public async Task<ActionResult<ApiResponse<bool>>> DismissReport(int reportId)
    {
        var result = await _reportService.DismissReportAsync(reportId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف المنشور المُبلَّغ عنه مع جميع ميديا وبلاغاته وملفاته الفعلية.
    /// يُستخدم لزر "Delete Post" في صفحة بلاغات الأدمن.
    /// </summary>
    /// <param name="reportId">رقم البلاغ المرتبط بالمنشور المراد حذفه.</param>
    /// <returns>true عند نجاح الحذف، أو خطأ 404.</returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Admin / ContentManager
    ///
    /// يحذف المنشور وجميع PostMedia والبلاغات المرتبطة به وجميع الملفات الفعلية.
    /// </remarks>
    [HttpDelete(ApiRoutes.CommunityPostReports.DeletePost)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReportedPost(int reportId)
    {
        var result = await _reportService.DeleteReportedPostAsync(reportId);
        return StatusCode(result.StatusCode, result);
    }
}
