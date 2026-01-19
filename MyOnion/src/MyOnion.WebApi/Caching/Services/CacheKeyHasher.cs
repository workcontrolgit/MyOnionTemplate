#nullable enable
using System.Security.Cryptography;
using System.Text;

namespace MyOnion.WebApi.Caching.Services;

public sealed class CacheKeyHasher : ICacheKeyHasher
{
    public string Hash(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(cacheKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
