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

        var response = new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            Roles = roles.ToArray(),
            Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["ExpiryMinutes"]))
        };

        return (true, "Login successful.", response);
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterOrganizationAsync(RegisterOrganizationDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return (false, "Email is already in use.", null);

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        string? uploadedLogoUrl = null;
        try
        {
            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

            var roleResult = await _userManager.AddToRoleAsync(user, IdentityRoles.Organization);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return (false, "Failed to assign role.", null);
            }

            var organization = new Organization
            {
                UserId = user.Id,
                OrgNameAr = dto.OrgNameAr,
                OrgNameEn = dto.OrgNameEn,
                DescriptionAr = dto.DescriptionAr,
                DescriptionEn = dto.DescriptionEn,
                AddressAr = dto.AddressAr,
                AddressEn = dto.AddressEn,
                MapUrl = dto.MapUrl,
                LogoUrl = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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

            var response = new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id,
                Roles = roles.ToArray(),
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["ExpiryMinutes"]))
            };

            return (true, "Organization registered successfully.", response);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            if (uploadedLogoUrl != null)
            {
                await _fileStorageService.DeleteFileAsync(uploadedLogoUrl);
            }
            return (false, "An error occurred during registration.", null);
        }
    }

    public async Task<(bool Success, string Message, AuthResponseDto? Data)> RegisterUserAsync(RegisterUserDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return (false, "Email is already in use.", null);

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

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
                {
                    await _fileStorageService.DeleteFileAsync(uploadedFileUrl);
                }
                await _userManager.DeleteAsync(user);
                return (false, "An error occurred while uploading profile image.", null);
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        var response = new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            Roles = roles.ToArray(),
            Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("Jwt")["ExpiryMinutes"]))
        };

        return (true, "User registered successfully.", response);
    }
}
