using Had360back.Data;
using Had360back.Dtos.Auth;
using Had360back.Models;
using Had360back.Common;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly DbContextApp _context;
    private readonly IFileStorageService _fileStorageService;

    public AuthService(UserManager<User> userManager, ITokenService tokenService, DbContextApp context, IFileStorageService fileStorageService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _fileStorageService = fileStorageService;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the JWT expiry from configuration.
    /// </summary>
    private DateTime BuildExpiry() =>
        DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("Jwt")["ExpiryMinutes"]));

    /// <summary>
    /// Creates a full AuthResponseDto from a user entity.
    /// Roles must already be retrieved.
    /// </summary>
    private AuthResponseDto BuildAuthResponse(User user, IList<string> roles, string token) =>
        new AuthResponseDto
        {
            Token      = token,
            Email      = user.Email!,
            FullName   = user.FullName,
            UserId     = user.Id,
            Roles      = roles.ToArray(),
            Expiration = BuildExpiry()
        };

    /// <summary>
    /// Sets an internal unique UserName in the format "user{id}" using UserManager
    /// so that NormalizedUserName is updated correctly by Identity.
    /// </summary>
    private async Task SetInternalUserNameAsync(User user)
    {
        await _userManager.SetUserNameAsync(user, $"user{user.Id}");
    }

    // ─── Login ───────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return (false, "Invalid email or password.", null);

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return (false, "Invalid email or password.", null);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return (true, "Login successful.", BuildAuthResponse(user, roles, token));
    }

    // ─── Register User ───────────────────────────────────────────────────────────

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterUserAsync(RegisterUserDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return (false, "Email is already in use.", null);

        // Use a guaranteed-unique temp username; will be replaced with "user{id}" after creation
        var user = new User
        {
            UserName    = $"temp_{Guid.NewGuid():N}",
            Email       = dto.Email,
            FullName    = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt   = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

        // Update UserName to the stable "user{id}" format via Identity (updates NormalizedUserName)
        await SetInternalUserNameAsync(user);

        var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.User);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return (false, "Failed to assign role.", null);
        }

        string? uploadedFileUrl = null;
        if (dto.ProfileImageFile != null)
        {
            try
            {
                uploadedFileUrl = await _fileStorageService.SaveUserProfileImageAsync(user.Id, dto.ProfileImageFile);
                user.ProfileImageUrl = uploadedFileUrl;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    await _fileStorageService.DeleteFileAsync(uploadedFileUrl);
                    await _userManager.DeleteAsync(user);
                    return (false, string.Join(", ", updateResult.Errors.Select(e => e.Description)), null);
                }
            }
            catch (Exception)
            {
                if (uploadedFileUrl != null)
                    await _fileStorageService.DeleteFileAsync(uploadedFileUrl);
                await _userManager.DeleteAsync(user);
                return (false, "An error occurred while uploading profile image.", null);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return (true, "User registered successfully.", BuildAuthResponse(user, roles, token));
    }

    // ─── Register Organization ───────────────────────────────────────────────────

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterOrganizationAsync(RegisterOrganizationDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return (false, "Email is already in use.", null);

        // FullName comes from the registration form (represents the account owner/representative).
        // Defensive fallback: if somehow empty, derive from org name then email.
        var fullName = !string.IsNullOrWhiteSpace(dto.FullName)
            ? dto.FullName
            : (!string.IsNullOrWhiteSpace(dto.OrgNameEn) ? dto.OrgNameEn : dto.Email);

        var user = new User
        {
            UserName    = $"temp_{Guid.NewGuid():N}",
            Email       = dto.Email,
            FullName    = fullName,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt   = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        string? uploadedLogoUrl = null;
        try
        {
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            // Set stable internal UserName after Id is assigned
            await SetInternalUserNameAsync(user);

            var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.Organization);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, "Failed to assign role.", null);
            }

            var organization = new Organization
            {
                UserId        = user.Id,
                OrgNameAr     = dto.OrgNameAr,
                OrgNameEn     = dto.OrgNameEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                AddressAr     = dto.AddressAr,
                AddressEn     = dto.AddressEn,
                MapUrl        = dto.MapUrl,
                LogoUrl       = null,
                CreatedAt     = DateTime.UtcNow,
                UpdatedAt     = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            if (dto.LogoFile != null)
            {
                uploadedLogoUrl = await _fileStorageService.SaveOrganizationLogoAsync(user.Id, dto.LogoFile);
                organization.LogoUrl = uploadedLogoUrl;
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);

            return (true, "Organization registered successfully.", BuildAuthResponse(user, roles, token));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            if (uploadedLogoUrl != null)
                await _fileStorageService.DeleteFileAsync(uploadedLogoUrl);
            return (false, "An error occurred during registration.", null);
        }
    }
}
