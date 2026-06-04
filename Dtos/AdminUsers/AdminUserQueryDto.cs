using System.ComponentModel.DataAnnotations;

namespace Had360back.Dtos.AdminUsers;

/// <summary>
/// Query parameters for paginated admin user list.
/// Supports filtering by search text and role.
/// </summary>
public class AdminUserQueryDto
{
    /// <summary>
    /// Optional free-text search across UserName, Email, and PhoneNumber.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Optional role filter. Must match an existing IdentityRole name
    /// (Admin, ContentManager, Organization, User).
    /// </summary>
    public string? Role { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;
}
