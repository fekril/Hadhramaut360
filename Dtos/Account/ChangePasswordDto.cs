using System.ComponentModel.DataAnnotations;

namespace Had360back.Dtos.Account;

/// <summary>
/// Input DTO for PUT /api/account/password.
/// Uses UserManager.ChangePasswordAsync internally so Identity validation applies.
/// </summary>
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = null!;
}
