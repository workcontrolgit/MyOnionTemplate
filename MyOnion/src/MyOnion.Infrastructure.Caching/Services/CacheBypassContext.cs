#nullable enable
namespace MyOnion.Infrastructure.Caching.Services;

public sealed class CacheBypassContext : ICacheBypassContext
{
    private string? _reason;

    public bool ShouldBypass => _reason is not null;

    public string? Reason => _reason;

    public void Enable(string reason)
    {
        _reason = string.IsNullOrWhiteSpace(reason) ? "unspecified" : reason;
    }

    public void Reset()
    {
        _reason = null;
    }
}
