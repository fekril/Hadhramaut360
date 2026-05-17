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

    public async Task<string> SaveLandmarkMediaAsync(int landmarkId, IFormFile file)
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

        // Validate size (10MB max)
        if (file.Length > 10 * 1024 * 1024)
            throw new ArgumentException("File size exceeds 10MB limit.");

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
