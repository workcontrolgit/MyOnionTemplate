using MyOnion.WebApi.Caching.Options;

namespace MyOnion.WebApi.Caching.Services;

internal static class CacheKeyFormatter
{
    public static string BuildCacheKey(CachingOptions options, string key)
    {
        var sanitized = string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim();
        return string.IsNullOrEmpty(sanitized)
            ? options.KeyPrefix
            : string.Concat(options.KeyPrefix, ":", sanitized);
    }

    public static string BuildPrefixKey(CachingOptions options, string prefix)
    {
        var sanitized = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.TrimEnd(':');
        return string.IsNullOrEmpty(sanitized)
            ? options.KeyPrefix
            : string.Concat(options.KeyPrefix, ":", sanitized);
    }
}
