using System.Threading.Tasks;
using Had360back.Common;
using Had360back.DTOs.CommunityPostReports;

namespace Had360back.Services.Interfaces;

/// <summary>
/// واجهة سيرفس إدارة بلاغات المنشورات المجتمعية (للأدمن فقط).
/// </summary>
public interface ICommunityPostReportService
{
    Task<ApiResponse<PaginatedResponse<CommunityPostReportResponseDto>>> GetAllReportsAsync(CommunityPostReportQueryDto query);
    Task<ApiResponse<bool>> DismissReportAsync(int reportId);
    Task<ApiResponse<bool>> DeleteReportedPostAsync(int reportId);
}
