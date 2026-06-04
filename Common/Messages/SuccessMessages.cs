namespace Had360back.Common.Messages;

/// <summary>
/// Centralized success messages for API responses.
/// Use these constants instead of hardcoded strings in new services.
/// </summary>
public static class SuccessMessages
{
    public const string RetrievedSuccessfully          = "Data retrieved successfully.";
    public const string CreatedSuccessfully            = "Created successfully.";
    public const string DeletedSuccessfully            = "Deleted successfully.";
    public const string UpdatedSuccessfully            = "Updated successfully.";
    public const string UserDeletedSuccessfully        = "User and all related data deleted successfully.";
    public const string ContentManagerCreatedSuccessfully = "Content manager account created successfully.";
}
