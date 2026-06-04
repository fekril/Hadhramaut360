using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Had360back.Services.Implementations;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    private void ValidateImage(IFormFile file, long maxSizeBytes)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        // Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException($"Invalid file extension: {extension}");

        // Validate content type
        string[] allowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException($"Invalid content type: {file.ContentType}");

        // Validate size
        if (file.Length > maxSizeBytes)
            throw new ArgumentException($"File size exceeds {maxSizeBytes / (1024 * 1024)}MB limit.");
    }

    public async Task<string> SaveLandmarkMediaAsync(int landmarkId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "landmarks", landmarkId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/landmarks/{landmarkId}/{uniqueFileName}";
    }

    public async Task<string> SaveOrganizationLogoAsync(int organizationUserId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "organizations", organizationUserId.ToString(), "logo");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/organizations/{organizationUserId}/logo/{uniqueFileName}";
    }

    public async Task<string> SaveUserProfileImageAsync(int userId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "users", userId.ToString(), "profile");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/users/{userId}/profile/{uniqueFileName}";
    }

    /// <summary>
    /// يحفظ ملف ميديا مرتبط بفعالية في مجلد التخزين المحلي.
    /// يتحقق من نوع الملف وحجمه قبل الحفظ.
    /// </summary>
    /// <param name="eventId">رقم الفعالية التي سيتم ربط الملف بها.</param>
    /// <param name="file">الملف المراد رفعه.</param>
    /// <returns>الرابط النسبي للملف المحفوظ.</returns>
    public async Task<string> SaveEventMediaAsync(int eventId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "events", eventId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/events/{eventId}/{uniqueFileName}";
    }

    /// <summary>
    /// يحفظ صورة غلاف مرتبطة بمحتوى اكتشاف في مجلد التخزين المحلي.
    /// يتحقق من نوع الملف وحجمه قبل الحفظ.
    /// </summary>
    /// <param name="contentId">رقم عنصر محتوى الاكتشاف الذي سيتم ربط الصورة به.</param>
    /// <param name="file">الملف المراد رفعه.</param>
    /// <returns>الرابط النسبي للملف المحفوظ.</returns>
    public async Task<string> SaveDiscoverContentCoverAsync(int contentId, IFormFile file)
    {
        ValidateImage(file, 5 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "discover-content", contentId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/discover-content/{contentId}/{uniqueFileName}";
    }

    /// <summary>
    /// يحفظ ملف ميديا مرتبط بحرفي في مجلد التخزين المحلي.
    /// يتحقق من نوع الملف وحجمه قبل الحفظ.
    /// </summary>
    /// <param name="artisanId">رقم الحرفي الذي سيتم ربط الملف به.</param>
    /// <param name="file">الملف المراد رفعه.</param>
    /// <returns>الرابط النسبي للملف المحفوظ.</returns>
    public async Task<string> SaveArtisanMediaAsync(int artisanId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "artisans", artisanId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/artisans/{artisanId}/{uniqueFileName}";
    }

    /// <summary>
    /// يحفظ ملف ميديا مرتبط بمنشور مجتمعي في مجلد التخزين المحلي.
    /// يتحقق من نوع الملف وحجمه قبل الحفظ.
    /// </summary>
    /// <param name="postId">رقم المنشور الذي سيتم ربط الملف به.</param>
    /// <param name="file">الملف المراد رفعه.</param>
    /// <returns>الرابط النسبي للملف المحفوظ.</returns>
    public async Task<string> SavePostMediaAsync(int postId, IFormFile file)
    {
        ValidateImage(file, 10 * 1024 * 1024);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "community-posts", postId.ToString());
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/community-posts/{postId}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || !fileUrl.StartsWith("/uploads/"))
            return Task.CompletedTask;

        try
        {
            // Convert web path to physical path safely
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_env.WebRootPath, relativePath);

            // Path traversal protection check
            var fullPath = Path.GetFullPath(filePath);
            var webRootFullPath = Path.GetFullPath(_env.WebRootPath);
            if (!fullPath.StartsWith(webRootFullPath))
                return Task.CompletedTask; // Abort if traversal attempted

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // If physical file deletion fails, do not crash the API
        }

        return Task.CompletedTask;
    }
}
