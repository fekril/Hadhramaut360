using System.Threading.Tasks;
using Had360back.Common;
using Had360back.Dtos.Dashboard;
using Had360back.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Had360back.Controllers;

/// <summary>
/// Provides admin dashboard statistics.
/// Accessible by Admin and ContentManager (ManageContent policy).
/// </summary>
[ApiController]
[Route(ApiRoutes.Dashboard.Base)]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns aggregate counts for all main entities in the system.
    /// Used by the admin dashboard cards.
    /// </summary>
    /// <returns>DashboardStatsDto with counts for users, organizations, reports, and content.</returns>
    /// <remarks>
    /// Method: GET
    /// Access: Admin, ContentManager
    /// </remarks>
    [HttpGet(ApiRoutes.Dashboard.Stats)]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        var result = await _dashboardService.GetStatsAsync();
        return StatusCode(result.StatusCode, result);
    }
}
