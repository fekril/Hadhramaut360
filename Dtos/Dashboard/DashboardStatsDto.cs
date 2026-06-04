namespace Had360back.Dtos.Dashboard;

/// <summary>
/// Response DTO for dashboard statistics cards.
/// Contains simple counts for each main entity in the system.
/// </summary>
public class DashboardStatsDto
{
    public int UsersCount          { get; set; }
    public int OrganizationsCount  { get; set; }
    public int ReportsCount        { get; set; }
    public int LandmarksCount      { get; set; }
    public int EventsCount         { get; set; }
    public int ArtisansCount       { get; set; }
    public int DiscoverContentCount { get; set; }
    public int CommunityPostsCount { get; set; }

    /// <summary>
    /// Sum of Landmarks + Events + Artisans + DiscoverContent + CommunityPosts.
    /// Useful for a "Total Content" dashboard card.
    /// </summary>
    public int TotalContentCount   { get; set; }
}
