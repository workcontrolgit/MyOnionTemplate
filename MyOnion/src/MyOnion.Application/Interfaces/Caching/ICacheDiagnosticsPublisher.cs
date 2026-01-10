#nullable enable
namespace MyOnion.Application.Interfaces.Caching;

public interface ICacheDiagnosticsPublisher
{
    void ReportHit();

    void ReportMiss();
}
