using System;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Data;
using Had360back.Dtos.Organizations;
using Had360back.Models;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// سيرفس لإدارة الملف الشخصي للمؤسسة.
/// </summary>
public class OrganizationService : IOrganizationService
{
    private readonly DbContextApp _context;
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public OrganizationService(
        DbContextApp context,
        UserManager<User> userManager,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _userManager = userManager;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// جلب بيانات الملف الشخصي للمؤسسة الحالية.
    /// </summary>
    /// <returns>الاستجابة المحتوية على بيانات الملف الشخصي للمؤسسة</returns>
    public async Task<ApiResponse<OrganizationProfileDto>> GetMeAsync()
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return ApiResponse<OrganizationProfileDto>.Fail("غير مصرح بالدخول.", statusCode: 401);
        }

        var organization = await _context.Organizations
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UserId == userId.Value);

        if (organization == null)
        {
            return ApiResponse<OrganizationProfileDto>.Fail("ملف المؤسسة غير موجود.", statusCode: 404);
        }

        var dto = MapToDto(organization);
        return ApiResponse<OrganizationProfileDto>.Ok(dto);
    }

    /// <summary>
    /// تحديث بيانات الملف الشخصي للمؤسسة الحالية.
    /// </summary>
    /// <param name="dto">DTO بيانات التحديث المرسلة</param>
    /// <returns>بيانات الملف الشخصي المحدثة أو رسالة خطأ</returns>
    public async Task<ApiResponse<OrganizationProfileDto>> UpdateMeAsync(UpdateOrganizationProfileDto dto)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return ApiResponse<OrganizationProfileDto>.Fail("غير مصرح بالدخول.", statusCode: 401);
        }

        var organization = await _context.Organizations
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UserId == userId.Value);

        if (organization == null)
        {
            return ApiResponse<OrganizationProfileDto>.Fail("ملف المؤسسة غير موجود.", statusCode: 404);
        }

        var user = organization.User;

        // تحديث رقم الهاتف للمستخدم المرتبط
        user.PhoneNumber = dto.PhoneNumber;
        var userUpdateResult = await _userManager.UpdateAsync(user);
        if (!userUpdateResult.Succeeded)
        {
            var errors = userUpdateResult.Errors.Select(e => e.Description).ToList();
            return ApiResponse<OrganizationProfileDto>.Fail("فشل تحديث بيانات المستخدم الملحق.", errors, statusCode: 400);
        }

        // تحديث حقول المؤسسة
        organization.OrgNameAr = dto.OrgNameAr;
        organization.OrgNameEn = dto.OrgNameEn;
        organization.DescriptionAr = dto.DescriptionAr;
        organization.DescriptionEn = dto.DescriptionEn;
        organization.AddressAr = dto.AddressAr;
        organization.AddressEn = dto.AddressEn;
        organization.MapUrl = dto.MapUrl;
        organization.UpdatedAt = DateTime.UtcNow;

        string? oldLogoUrl = organization.LogoUrl;
        string? newLogoUrl = null;

        if (dto.LogoFile != null)
        {
            try
            {
                newLogoUrl = await _fileStorageService.SaveOrganizationLogoAsync(userId.Value, dto.LogoFile);
            }
            catch (Exception ex)
            {
                return ApiResponse<OrganizationProfileDto>.Fail($"فشل رفع شعار المؤسسة: {ex.Message}", statusCode: 400);
            }
            organization.LogoUrl = newLogoUrl;
        }

        try
        {
            await _context.SaveChangesAsync();

            // عند نجاح التخزين وحفظ البيانات، نقوم بحذف الشعار القديم إن وجد
            if (dto.LogoFile != null && !string.IsNullOrEmpty(oldLogoUrl))
            {
                await _fileStorageService.DeleteFileAsync(oldLogoUrl);
            }
        }
        catch (Exception)
        {
            // عند فشل حفظ البيانات في قاعدة البيانات، نقوم بحذف الملف الجديد المرفوع
            if (!string.IsNullOrEmpty(newLogoUrl))
            {
                await _fileStorageService.DeleteFileAsync(newLogoUrl);
            }
            return ApiResponse<OrganizationProfileDto>.Fail("حدث خطأ أثناء حفظ التحديثات في قاعدة البيانات.", statusCode: 500);
        }

        var updatedDto = MapToDto(organization);
        return ApiResponse<OrganizationProfileDto>.Ok(updatedDto, "تم تحديث الملف الشخصي بنجاح.");
    }

    private static OrganizationProfileDto MapToDto(Organization organization)
    {
        return new OrganizationProfileDto
        {
            UserId = organization.UserId,
            Email = organization.User.Email,
            PhoneNumber = organization.User.PhoneNumber,
            OrgNameAr = organization.OrgNameAr,
            OrgNameEn = organization.OrgNameEn,
            DescriptionAr = organization.DescriptionAr,
            DescriptionEn = organization.DescriptionEn,
            AddressAr = organization.AddressAr,
            AddressEn = organization.AddressEn,
            MapUrl = organization.MapUrl,
            LogoUrl = organization.LogoUrl,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt
        };
    }
}
