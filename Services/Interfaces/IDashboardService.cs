using Had360back.Common;
using Had360back.Dtos.Dashboard;
using System.Threading.Tasks;

namespace Had360back.Services.Interfaces;

/// <summary>
/// Service interface for admin dashboard statistics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns aggregate counts for each main entity in the system.
    /// </summary>
    Task<ApiResponse<DashboardStatsDto>> GetStatsAsync();
}
