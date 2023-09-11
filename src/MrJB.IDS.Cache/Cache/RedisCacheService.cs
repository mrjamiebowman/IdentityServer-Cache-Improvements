using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MrJB.IDS.Cache.Configuration;
using System.Text.Json;

namespace MrJB.IDS.Cache;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;

    private readonly CacheConfiguration _cacheConfiguration;

    public RedisCacheService(IDistributedCache cache, IOptions<CacheConfiguration> cacheConfiguration)
    {
        _cache = cache;
        _cacheConfiguration = cacheConfiguration.Value;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        using var activity = OTel.Application.StartActivity("RedisCacheService.GetAsync");

        var value = await _cache.GetStringAsync(key);

        if (value != null)
        {
            return JsonSerializer.Deserialize<T>(value);
        }

        return default;
    }

    public async Task<T> GetAsync<T>(List<string> keys)
    {
        using var activity = OTel.Application.StartActivity("RedisCacheService.GetAsync");

        //var value = await _cache.GetStringAsync(keys);

        //if (value != null)
        //{
        //    return JsonSerializer.Deserialize<T>(value);
        //}

        return default;
    }

    public async Task RemoveAsync(string key)
    {
        using var activity = OTel.Application.StartActivity("RedisCacheService.RemoveAsync");
        await _cache.RemoveAsync(key);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        using var activity = OTel.Application.StartActivity("RedisCacheService.SetAsync");
        
        string data = JsonSerializer.Serialize(value);

        // vars
        var absExpRelTime = _cacheConfiguration.AbsoluteExpirationRelativeToNow != null ? _cacheConfiguration.AbsoluteExpirationRelativeToNow : null;

        var timeOut = new DistributedCacheEntryOptions {
            AbsoluteExpirationRelativeToNow = absExpRelTime,
            SlidingExpiration = _cacheConfiguration.SlidingExpiration
        };

        await _cache.SetStringAsync(key, data, timeOut);
    }
}
