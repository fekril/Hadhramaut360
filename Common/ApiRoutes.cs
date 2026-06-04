namespace Had360back.Common;

public static class ApiRoutes
{
    public const string Root = "api";

    public static class Categories
    {
        public const string Base = Root + "/categories";
        public const string ById = "{id:int}";
    }

    public static class Landmarks
    {
        public const string Base = Root + "/landmarks";
        public const string ById = "{id:int}";
        public const string WithMedia = "with-media";
        public const string AddMedia = "{id:int}/media";
        public const string UpdateMedia = "media/{mediaId:int}";
        public const string DeleteMedia = "media/{mediaId:int}";
    }

    public static class Favorites
    {
        public const string Base = Root + "/favorites";
        public const string My = "my";
        public const string ByLandmarkId = "{landmarkId:int}";
        public const string Status = "{landmarkId:int}/status";
        public const string Toggle = "{landmarkId:int}/toggle";
    }

    public static class Organizations
    {
        public const string Base = Root + "/organizations";
        public const string Me = "me";
    }

    /// <summary>
    /// مسارات API الخاصة بالفعاليات.
    /// </summary>
    public static class Events
    {
        public const string Base = Root + "/events";
        public const string ById = "{id:int}";
        public const string Edit = "{id:int}/edit";
        public const string My = "my";
        public const string WithMedia = "with-media";
        public const string AddMedia = "{id:int}/media";
        public const string UpdateMedia = "media/{mediaId:int}";
        public const string DeleteMedia = "media/{mediaId:int}";
    }

    /// <summary>
    /// مسارات API الخاصة بمحتوى الاكتشاف.
    /// </summary>
    public static class DiscoverContent
    {
        public const string Base    = Root + "/discover-content";
        public const string ById    = "{id:int}";
        public const string Culture = "culture";
        public const string Food    = "food";
        public const string Games   = "games";
    }

    /// <summary>
    /// مسارات API الخاصة بالحرفيين.
    /// </summary>
    public static class Artisans
    {
        public const string Base       = Root + "/artisans";
        public const string ById       = "{id:int}";
        public const string WithMedia  = "with-media";
        public const string AddMedia   = "{id:int}/media";
        public const string UpdateMedia = "media/{mediaId:int}";
        public const string DeleteMedia = "media/{mediaId:int}";
        public const string SetCover   = "media/{mediaId:int}/cover";
    }

    /// <summary>
    /// مسارات API الخاصة بمنشورات المجتمع.
    /// </summary>
    public static class CommunityPosts
    {
        public const string Base        = Root + "/community-posts";
        public const string ById        = "{id:int}";
        public const string WithMedia   = "with-media";
        public const string Report      = "{id:int}/report";
        public const string AddMedia    = "{id:int}/media";
        public const string UpdateMedia = "media/{mediaId:int}";
        public const string DeleteMedia = "media/{mediaId:int}";
    }

    /// <summary>
    /// مسارات API الخاصة بتقارير بلاغات المنشورات (للأدمن فقط).
    /// </summary>
    public static class CommunityPostReports
    {
        public const string Base            = Root + "/community-post-reports";
        public const string ByReportId      = "{reportId:int}";
        public const string DeletePost      = "{reportId:int}/post";
    }

    /// <summary>
    /// Dashboard stats routes (Admin and ContentManager).
    /// </summary>
    public static class Dashboard
    {
        public const string Base  = Root + "/dashboard";
        public const string Stats = "stats";
    }

    /// <summary>
    /// Admin user management routes (Admin only).
    /// </summary>
    public static class AdminUsers
    {
        public const string Base            = Root + "/admin/users";
        public const string ById            = "{id:int}";
        public const string ContentManagers = "content-managers";
    }
}

