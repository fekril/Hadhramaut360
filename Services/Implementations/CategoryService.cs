using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.DTOs.Categories;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// Service مسؤول عن تنفيذ منطق إدارة تصنيفات المعالم.
/// يتعامل مع قاعدة البيانات لإضافة التصنيفات، عرضها، تعديلها، وحذفها.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly DbContextApp _context;

    public CategoryService(DbContextApp context)
    {
        _context = context;
    }

    /// <summary>
    /// يجلب جميع التصنيفات من قاعدة البيانات.
    /// </summary>
    /// <returns>
    /// ApiResponse يحتوي على قائمة من CategoryResponseDto مرتبة حسب الاسم العربي.
    /// </returns>
    /// <remarks>
    /// تستخدم هذه الدالة لعرض التصنيفات في الواجهة الأمامية،
    /// خصوصًا في قوائم الفلترة الخاصة بالمعالم.
    /// يتم تحويل كل كائن Category إلى CategoryResponseDto قبل إرجاعه.
    /// </remarks>
    public async Task<ApiResponse<List<CategoryResponseDto>>> GetAllAsync()
    {
        var categories = await _context.Set<Category>()
            .OrderBy(c => c.CategoryNameAr)
            .Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryNameAr = c.CategoryNameAr,
                CategoryNameEn = c.CategoryNameEn,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<CategoryResponseDto>>.Ok(categories);
    }


    /// <summary>
    /// ينشئ تصنيفًا جديدًا في قاعدة البيانات.
    /// </summary>
    /// <param name="dto">
    /// يحتوي على اسم التصنيف باللغة العربية واسم التصنيف باللغة الإنجليزية.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على بيانات التصنيف الجديد عند نجاح الإنشاء.
    /// يرجع خطأ إذا كان يوجد تصنيف آخر بنفس الاسم العربي أو الاسم الإنجليزي.
    /// </returns>
    /// <remarks>
    /// تتحقق الدالة أولًا من عدم وجود تصنيف بنفس الاسم العربي أو الإنجليزي.
    /// بعد ذلك تنشئ كائن Category جديد وتخزنه في قاعدة البيانات.
    /// يتم تعيين CreatedAt تلقائيًا باستخدام DateTime.UtcNow.
    /// </remarks>
    public async Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto)
    {
        var exists = await _context.Set<Category>().AnyAsync(c =>
            c.CategoryNameAr.ToLower() == dto.CategoryNameAr.ToLower() ||
            c.CategoryNameEn.ToLower() == dto.CategoryNameEn.ToLower());

        if (exists)
            return ApiResponse<CategoryResponseDto>.Fail("Category with the same Arabic or English name already exists.");

        var category = new Category
        {
            CategoryNameAr = dto.CategoryNameAr,
            CategoryNameEn = dto.CategoryNameEn,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Category>().Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryNameAr = category.CategoryNameAr,
            CategoryNameEn = category.CategoryNameEn,
            CreatedAt = category.CreatedAt
        };

        return ApiResponse<CategoryResponseDto>.Ok(response, "Category created successfully", 201);
    }


    /// <summary>
    /// يحدث بيانات تصنيف موجود في قاعدة البيانات.
    /// </summary>
    /// <param name="id">
    /// رقم التصنيف المراد تحديثه.
    /// </param>
    /// <param name="dto">
    /// يحتوي على الاسم العربي والاسم الإنجليزي الجديدين للتصنيف.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على بيانات التصنيف بعد التحديث.
    /// يرجع خطأ 404 إذا لم يتم العثور على التصنيف.
    /// ويرجع خطأ إذا كان يوجد تصنيف آخر بنفس الاسم العربي أو الإنجليزي.
    /// </returns>
    /// <remarks>
    /// تبحث الدالة أولًا عن التصنيف باستخدام رقمه.
    /// ثم تتحقق من عدم وجود تصنيف آخر بنفس الاسم العربي أو الإنجليزي.
    /// بعد ذلك تحدث بيانات التصنيف وتحفظ التغييرات في قاعدة البيانات.
    /// </remarks>
    public async Task<ApiResponse<CategoryResponseDto>> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Set<Category>().FindAsync(id);
        if (category == null)
            return ApiResponse<CategoryResponseDto>.Fail("Category not found.", null, 404);

        var exists = await _context.Set<Category>().AnyAsync(c =>
            c.CategoryId != id &&
            (c.CategoryNameAr.ToLower() == dto.CategoryNameAr.ToLower() ||
             c.CategoryNameEn.ToLower() == dto.CategoryNameEn.ToLower()));

        if (exists)
            return ApiResponse<CategoryResponseDto>.Fail("Another category with the same Arabic or English name already exists.");

        category.CategoryNameAr = dto.CategoryNameAr;
        category.CategoryNameEn = dto.CategoryNameEn;

        await _context.SaveChangesAsync();

        var response = new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryNameAr = category.CategoryNameAr,
            CategoryNameEn = category.CategoryNameEn,
            CreatedAt = category.CreatedAt
        };

        return ApiResponse<CategoryResponseDto>.Ok(response, "Category updated successfully");
    }


    /// <summary>
    /// يحذف تصنيفًا من قاعدة البيانات إذا لم يكن مرتبطًا بأي معلم.
    /// </summary>
    /// <param name="id">
    /// رقم التصنيف المراد حذفه.
    /// </param>
    /// <returns>
    /// ApiResponse يحتوي على true عند نجاح الحذف.
    /// يرجع خطأ 404 إذا لم يتم العثور على التصنيف.
    /// ويرجع خطأ إذا كان التصنيف مرتبطًا بمعلم واحد أو أكثر.
    /// </returns>
    /// <remarks>
    /// تقوم الدالة بجلب التصنيف مع العلاقات المرتبطة بالمعالم.
    /// إذا كان التصنيف مرتبطًا بمعالم، تمنع عملية الحذف لحماية سلامة البيانات.
    /// إذا لم يكن مرتبطًا، يتم حذف التصنيف من قاعدة البيانات.
    /// </remarks>
    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var category = await _context.Set<Category>()
            .Include(c => c.LandmarkCategories)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
            return ApiResponse<bool>.Fail("Category not found.", null, 404);

        if (category.LandmarkCategories.Any())
            return ApiResponse<bool>.Fail("Category cannot be deleted because it is associated with one or more landmarks.");

        _context.Set<Category>().Remove(category);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Category deleted successfully");
    }
}
