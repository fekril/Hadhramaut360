using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Had360back.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveLandmarkMediaAsync(int landmarkId, IFormFile file);
    Task<string> SaveOrganizationLogoAsync(int organizationUserId, IFormFile file);
    Task<string> SaveUserProfileImageAsync(int userId, IFormFile file);
    Task DeleteFileAsync(string? fileUrl);
}
