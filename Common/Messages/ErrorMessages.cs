namespace Had360back.Common.Messages;

/// <summary>
/// Centralized error messages for API responses.
/// Use these constants instead of hardcoded strings in new services.
/// </summary>
public static class ErrorMessages
{
    public const string NotFound             = "Resource not found.";
    public const string UserNotFound         = "User not found.";
    public const string Unauthorized         = "Unauthorized access.";
    public const string Forbidden            = "You do not have permission to perform this action.";
    public const string InvalidRole          = "The specified role is invalid.";
    public const string EmailAlreadyExists   = "A user with this email address already exists.";
    public const string CannotDeleteYourself = "You cannot delete your own account.";
    public const string CannotDeleteLastAdmin = "Cannot delete the last remaining Admin account.";
    public const string DeleteUserFailed     = "Failed to delete the user. Please try again.";
    public const string UnexpectedError      = "An unexpected error occurred. Please try again later.";
}
