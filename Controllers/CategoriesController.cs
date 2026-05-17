using System.Collections.Generic;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.Categories;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;
/// <summary>
/// Controller مسؤول عن إدارة واجهات API الخاصة بتصنيفات المعالم.
/// يوفر عمليات عرض التصنيفات، إضافة تصنيف جديد، تعديل تصنيف، وحذف تصنيف.
/// </summary>
[ApiController]
[Route(ApiRoutes.Categories.Base)]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// يجلب جميع التصنيفات الموجودة في النظام.
    /// </summary>
    /// <returns>
    /// يرجع ApiResponse يحتوي على قائمة من CategoryResponseDto.
    /// </returns>
    /// <remarks>
    /// Method: GET
    /// Access: Public
    ///
    /// يستخدم هذا الـ Endpoint لعرض التصنيفات في واجهة المستخدم،
    /// مثل استخدامها في فلترة المعالم حسب نوع التصنيف.
    /// </remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// ينشئ تصنيفًا جديدًا للمعالم.
    /// </summary>
    /// <param name="dto">
    /// يحتوي على اسم التصنيف باللغة العربية واسم التصنيف باللغة الإنجليزية.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات التصنيف الجديد بعد إنشائه.
    /// </returns>
    /// <remarks>
    /// Method: POST
    /// Access: Requires ManageContent policy
    /// Body Type: JSON
    ///
    /// هذا الـ Endpoint مخصص للمستخدمين الذين لديهم صلاحية إدارة المحتوى.
    /// يتم استخدامه لإضافة تصنيفات جديدة مثل: معالم تاريخية، متاحف، شواطئ.
    /// </remarks>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return StatusCode(result.StatusCode, result);
    }


    /// <summary>
    /// يحدث بيانات تصنيف موجود حسب رقم التصنيف.
    /// </summary>
    /// <param name="id">
    /// رقم التصنيف المراد تعديله.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الاسم العربي والاسم الإنجليزي الجديدين للتصنيف.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على بيانات التصنيف بعد التحديث.
    /// في حالة عدم وجود التصنيف يرجع خطأ 404.
    /// </returns>
    /// <remarks>
    /// Method: PUT
    /// Access: Requires ManageContent policy
    /// Body Type: JSON
    ///
    /// يستخدم هذا الـ Endpoint لتعديل أسماء التصنيف.
    /// لا يغير هذا الـ Endpoint المعالم المرتبطة بالتصنيف.
    /// </remarks>
    [HttpPut(ApiRoutes.Categories.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return StatusCode(result.StatusCode, result);
    }
    /// <summary>
    /// يحذف تصنيفًا موجودًا حسب رقم التصنيف.
    /// </summary>
    /// <param name="id">
    /// رقم التصنيف المراد حذفه.
    /// </param>
    /// <returns>
    /// يرجع ApiResponse يحتوي على true عند نجاح الحذف.
    /// في حالة عدم وجود التصنيف يرجع خطأ 404.
    /// </returns>
    /// <remarks>
    /// Method: DELETE
    /// Access: Requires ManageContent policy
    ///
    /// يستخدم هذا الـ Endpoint لحذف تصنيف غير مرتبط بأي معلم.
    /// إذا كان التصنيف مرتبطًا بمعلم واحد أو أكثر، فلن يتم حذفه لحماية سلامة البيانات.
    /// </remarks>
    [HttpDelete(ApiRoutes.Categories.ById)]
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return StatusCode(result.StatusCode, result);
    }
}
