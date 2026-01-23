using MyOnion.Application.Features.Dashboard.Queries.GetDashboardMetrics;

namespace MyOnion.Application.Interfaces;

public interface IDashboardMetricsReader
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct);
}
