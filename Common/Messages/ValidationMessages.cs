namespace Had360back.Common.Messages;

/// <summary>
/// Centralized validation messages for API responses.
/// Use these constants instead of hardcoded strings in new services.
/// </summary>
public static class ValidationMessages
{
    public const string PageNumberInvalid = "Page number must be greater than or equal to 1.";
    public const string PageSizeInvalid   = "Page size must be between 1 and 50.";
    public const string RequiredField     = "This field is required.";
    public const string InvalidEmail      = "The email address format is invalid.";
    public const string PasswordRequired  = "Password is required.";
    public const string RoleInvalid       = "The provided role is not valid. Allowed values: Admin, ContentManager, Organization, User.";
}
