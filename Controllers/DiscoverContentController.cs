using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.DiscoverContent;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Controller مسؤول عن جميع واجهات API الخاصة بمحتوى الاكتشاف.
/// يوفر عمليات العرض العام، عرض التفاصيل، الأقسام القائمة على الكلمات المفتاحية، والإدارة (إنشاء، تعديل، حذف).
/// </summary>
[ApiController]
[Route(ApiRoutes.DiscoverContent.Base)]
public class DiscoverContentController : ControllerBase
{
    private readonly IDiscoverContentService _discoverContentService;

    public DiscoverContentController(IDiscoverContentService discoverContentService)
    {
        _discoverContentService = discoverContentService;
    }

    // ─── Static keyword lists for each section ──────────────────────────────────
    // Adding a new section requires only: a new endpoint method + a new keyword list here.

    private static class SectionKeywords
    {
        public static readonly IReadOnlyList<string> Food = new[]
        {
            "أكل", "أكلات", "لذيذ", "طبخ", "كبسة", "حلويات",
            "food", "cuisine", "dish", "meal", "recipe"
        };

        public static readonly IReadOnlyList<string> Culture = new[]
        {
            "ثقافة", "تراث", "عادات", "حضارة", "موروث",
            "culture", "heritage", "tradition", "history"
        };

        public static readonly IReadOnlyList<string> Games = new[]
        {
            "لعبة", "ألعاب", "ألعاب شعبية", "تراثية",
            "game", "games", "traditional game", "play"
        };
    }

    // ─── Public Endpoints ────────────────────────────────────────────────────────

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف للعموم مع دعم البحث والتقسيم إلى صفحات.
    /// </summary>
    /// <param name="query">معاملات البحث والتقسيم إلى صفحات.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة DiscoverContentResponseDto مقسّمة إلى صفحات.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint لعرض صفحة محتوى الاكتشاف العامة.
    /// يبحث في العنوان والمحتوى بكلا اللغتين (العربية والإنجليزية).
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>>> GetAll(
        [FromQuery] DiscoverContentQueryDto query)
    {
        var result = await _discoverContentService.GetAllAsync(query);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب تفاصيل عنصر محتوى اكتشاف واحد حسب رقمه.
    /// </summary>
    /// <param name="id">رقم عنصر المحتوى المطلوب.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على DiscoverContentDetailsDto عند العثور على العنصر.
    /// يرجع 404 إذا لم يوجد العنصر.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint في صفحة تفاصيل محتوى الاكتشاف.
    /// يعرض المحتوى الكامل بكلا اللغتين.
    /// </remarks>
    [HttpGet(ApiRoutes.DiscoverContent.ById)]
    public async Task<ActionResult<ApiResponse<DiscoverContentDetailsDto>>> GetById(int id)
    {
        var result = await _discoverContentService.GetByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف الخاصة بقسم الثقافة.
    /// يعمل بنفس آلية قائمة الاكتشاف العامة مع فلترة مسبقة حسب الكلمات المفتاحية الثقافية.
    /// </summary>
    /// <param name="query">معاملات البحث الإضافي والتقسيم إلى صفحات.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة DiscoverContentResponseDto المرتبطة بالثقافة.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint عند الضغط على زر "ثقافة" في الفرونت إند.
    /// الكلمات المفتاحية المستخدمة: ثقافة، تراث، عادات، حضارة، culture، heritage، tradition.
    /// </remarks>
    [HttpGet(ApiRoutes.DiscoverContent.Culture)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>>> GetCulture(
        [FromQuery] DiscoverContentQueryDto query)
    {
        var result = await _discoverContentService.GetBySectionAsync(query, SectionKeywords.Culture);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف الخاصة بقسم الأكل.
    /// يعمل بنفس آلية قائمة الاكتشاف العامة مع فلترة مسبقة حسب الكلمات المفتاحية للأكل.
    /// </summary>
    /// <param name="query">معاملات البحث الإضافي والتقسيم إلى صفحات.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة DiscoverContentResponseDto المرتبطة بالأكل.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint عند الضغط على زر "أكل" في الفرونت إند.
    /// الكلمات المفتاحية المستخدمة: أكل، أكلات، لذيذ، طبخ، كبسة، حلويات، food، cuisine، dish.
    /// </remarks>
    [HttpGet(ApiRoutes.DiscoverContent.Food)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>>> GetFood(
        [FromQuery] DiscoverContentQueryDto query)
    {
        var result = await _discoverContentService.GetBySectionAsync(query, SectionKeywords.Food);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يجلب قائمة محتوى الاكتشاف الخاصة بقسم الألعاب.
    /// يعمل بنفس آلية قائمة الاكتشاف العامة مع فلترة مسبقة حسب الكلمات المفتاحية للألعاب.
    /// </summary>
    /// <param name="query">معاملات البحث الإضافي والتقسيم إلى صفحات.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة DiscoverContentResponseDto المرتبطة بالألعاب.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint عند الضغط على زر "ألعاب" في الفرونت إند.
    /// الكلمات المفتاحية المستخدمة: لعبة، ألعاب، ألعاب شعبية، تراثية، game، traditional game.
    /// </remarks>
    [HttpGet(ApiRoutes.DiscoverContent.Games)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<DiscoverContentResponseDto>>>> GetGames(
        [FromQuery] DiscoverContentQueryDto query)
    {
        var result = await _discoverContentService.GetBySectionAsync(query, SectionKeywords.Games);
        return StatusCode(result.StatusCode, result);
    }

    // ─── Admin / ContentManager Endpoints ───────────────────────────────────────

    /// <summary>
    /// ينشئ عنصر محتوى اكتشاف جديدًا.
    /// </summary>
    /// <param name="dto">بيانات العنصر الجديد بصيغة JSON.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل العنصر بعد إنشائه برمز 201.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy (Admin or ContentManager)
    /// Body Type: JSON
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<DiscoverContentDetailsDto>>> Create(
        [FromForm] CreateDiscoverContentDto dto)
    {
        var result = await _discoverContentService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحدث عنصر محتوى اكتشاف موجودًا حسب رقمه.
    /// </summary>
    /// <param name="id">رقم العنصر المراد تعديله.</param>
    /// <param name="dto">البيانات الجديدة للعنصر بصيغة JSON.</param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على تفاصيل العنصر بعد التعديل.
    /// يرجع 404 إذا لم يوجد العنصر.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy (Admin or ContentManager)
    /// Body Type: JSON
    /// </remarks>
    [HttpPut(ApiRoutes.DiscoverContent.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<DiscoverContentDetailsDto>>> Update(
        int id, [FromForm] UpdateDiscoverContentDto dto)
    {
        var result = await _discoverContentService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// يحذف عنصر محتوى اكتشاف موجودًا حسب رقمه.
    /// </summary>
    /// <param name="id">رقم العنصر المراد حذفه.</param>
    /// <returns>
    /// يرجع ApiResponse بقيمة true عند نجاح الحذف.
    /// يرجع 404 إذا لم يوجد العنصر.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy (Admin or ContentManager)
    /// </remarks>
    [HttpDelete(ApiRoutes.DiscoverContent.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _discoverContentService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
