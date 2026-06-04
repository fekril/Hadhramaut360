using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Had360back.Dtos.Account;

/// <summary>
/// Input DTO for PUT /api/account/me.
/// Allows updating FullName, PhoneNumber, and profile image.
/// UserName and Email are NOT changeable here.
/// </summary>
public class UpdateAccountProfileDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
    public string FullName { get; set; } = null!;

    [Phone(ErrorMessage = "Phone number format is invalid.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Optional new profile image. If provided the old image is deleted after DB save succeeds.
    /// </summary>
    public IFormFile? ProfileImageFile { get; set; }
}
