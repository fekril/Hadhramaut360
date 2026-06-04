using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Had360back.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveLandmarkMediaAsync(int landmarkId, IFormFile file);
    Task<string> SaveOrganizationLogoAsync(int organizationUserId, IFormFile file);
    Task<string> SaveUserProfileImageAsync(int userId, IFormFile file);
    Task<string> SaveEventMediaAsync(int eventId, IFormFile file);
    Task<string> SaveDiscoverContentCoverAsync(int contentId, IFormFile file);
    Task<string> SaveArtisanMediaAsync(int artisanId, IFormFile file);
    Task<string> SavePostMediaAsync(int postId, IFormFile file);
    Task DeleteFileAsync(string? fileUrl);
}
