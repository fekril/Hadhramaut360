namespace Had360back.Common;

public static class ApiRoutes
{
    public const string Root = "Had360";

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
}

