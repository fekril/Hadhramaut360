using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Common.Messages;
using Had360back.Data;
using Had360back.Dtos.Dashboard;
using Had360back.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Had360back.Services.Implementations;

/// <summary>
/// Implements dashboard statistics by running sequential CountAsync queries
/// against the database.
///
/// IMPORTANT: EF Core DbContext is NOT thread-safe.
/// Task.WhenAll on the same DbContext causes:
/// "A second operation was started on this context before a previous operation completed."
/// All queries must be awaited one at a time.
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
        // Each query is awaited separately — EF Core DbContext is not thread-safe
        // and cannot handle concurrent async operations on the same instance.
        var usersCount           = await _context.Users.CountAsync();
        var organizationsCount   = await _context.Organizations.CountAsync();
        var reportsCount         = await _context.Reports.CountAsync();
        var landmarksCount       = await _context.Landmarks.CountAsync();
        var eventsCount          = await _context.Events.CountAsync();
        var artisansCount        = await _context.Artisans.CountAsync();
        var discoverContentCount = await _context.DiscoverContent.CountAsync();
        var communityPostsCount  = await _context.CommunityPosts.CountAsync();

        var stats = new DashboardStatsDto
        {
            UsersCount           = usersCount,
            OrganizationsCount   = organizationsCount,
            ReportsCount         = reportsCount,
            LandmarksCount       = landmarksCount,
            EventsCount          = eventsCount,
            ArtisansCount        = artisansCount,
            DiscoverContentCount = discoverContentCount,
            CommunityPostsCount  = communityPostsCount,
            TotalContentCount    = landmarksCount + eventsCount + artisansCount
                                   + discoverContentCount + communityPostsCount
        };

        return ApiResponse<DashboardStatsDto>.Ok(stats, SuccessMessages.RetrievedSuccessfully);
    }
}
