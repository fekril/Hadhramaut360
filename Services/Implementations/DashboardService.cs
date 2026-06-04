using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Common.Messages;
using Had360back.Data;
using Had360back.Dtos.Dashboard;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// Implements dashboard statistics by running efficient CountAsync queries
/// against the database. No sensitive data is exposed.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly DbContextApp _context;

    public DashboardService(DbContextApp context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApiResponse<DashboardStatsDto>> GetStatsAsync()
    {
        // Run all counts concurrently for best performance
        var usersCountTask          = _context.Users.CountAsync();
        var organizationsCountTask  = _context.Organizations.CountAsync();
        var reportsCountTask        = _context.Reports.CountAsync();
        var landmarksCountTask      = _context.Landmarks.CountAsync();
        var eventsCountTask         = _context.Events.CountAsync();
        var artisansCountTask       = _context.Artisans.CountAsync();
        var discoverContentCountTask = _context.DiscoverContent.CountAsync();
        var communityPostsCountTask = _context.CommunityPosts.CountAsync();

        await Task.WhenAll(
            usersCountTask,
            organizationsCountTask,
            reportsCountTask,
            landmarksCountTask,
            eventsCountTask,
            artisansCountTask,
            discoverContentCountTask,
            communityPostsCountTask);

        var landmarks      = landmarksCountTask.Result;
        var events         = eventsCountTask.Result;
        var artisans       = artisansCountTask.Result;
        var discoverContent = discoverContentCountTask.Result;
        var communityPosts = communityPostsCountTask.Result;

        var stats = new DashboardStatsDto
        {
            UsersCount           = usersCountTask.Result,
            OrganizationsCount   = organizationsCountTask.Result,
            ReportsCount         = reportsCountTask.Result,
            LandmarksCount       = landmarks,
            EventsCount          = events,
            ArtisansCount        = artisans,
            DiscoverContentCount = discoverContent,
            CommunityPostsCount  = communityPosts,
            TotalContentCount    = landmarks + events + artisans + discoverContent + communityPosts
        };

        return ApiResponse<DashboardStatsDto>.Ok(stats, SuccessMessages.RetrievedSuccessfully);
    }
}
