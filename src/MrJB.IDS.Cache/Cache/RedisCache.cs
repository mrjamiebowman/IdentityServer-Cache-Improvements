using Duende.IdentityServer.Services;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Text.Json;

namespace MrJB.IDS.Cache;

public class RedisCache<T> : ICache<T> where T : class
{
    // logging
    private readonly ILogger<RedisCache<T>> _logger;

    // service
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    // type
    private object _type = typeof(T);

    public IDatabase Db
    {
        get
        {
            var db = _connectionMultiplexer.GetDatabase();
            return db;
        }
    }

    public RedisCache(ILogger<RedisCache<T>> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T> GetAsync(string key)
    {
        using var activity = OTel.Application.StartActivity("RedisCache.GetAsync");

        var cacheKey = _type.ToString() + key;

        try
        {
            var item = await Db.StringGetAsync(cacheKey);

            if (!item.IsNullOrEmpty)
            {
                _logger.LogDebug("Retrieved {type} with Key: {key} from Redis Cache successfully.", _type.ToString(), key);
                return JsonSerializer.Deserialize<T>(item);
            }
        } catch (Exception ex)
        {
            // don't fail
            _logger.LogError("Unable to retrive {type} with Key: {key} from Redis Cache because of error", _type.ToString(), key, ex.Message);
            activity?.RecordException(ex);
        }

        _logger.LogDebug("Missed {type} with Key: {key} from Redis Cache.", _type.ToString(), key);
        return default(T);
    }

    public async Task<Dictionary<string, T>> GetAsync(List<string> keys)
    {
        using var activity = OTel.Application.StartActivity("RedisCache.GetAsync");

        try
        {
            var tasks = new List<Task>();
            var items = new Dictionary<string, T>();

            // keys
            List<RedisKey> redisKeys = new List<RedisKey>();
            keys.ForEach(x => redisKeys.Add(new RedisKey(_type.ToString() + x)));

            // redis supports retrieving multiple keys in one call.. other cache's may not.. this can be accommodated through their implementation.
            var redisValues = await Db.StringGetAsync(redisKeys.ToArray());

            int i = -1;
            foreach (var key in redisValues)
            {
                i++;
                if (key.HasValue)
                {
                    items.Add(keys[i], JsonSerializer.Deserialize<T>(key));
                }
            }

            return items;
        }
        catch (Exception ex)
        {
            // don't crash the application when the redis cache is not available, just log the error and continue, the value will be retrieved from the database instead of cache
            _logger.LogError("Unable to retrieve {type} with Key: {key} from Redis Cache because of connection error {redisConnectionException}.", _type.ToString(), keys, ex.Message);
            activity?.RecordException(ex);
        }

        _logger.LogDebug("Missed {type} with Key: {key} from Redis Cache.", _type.ToString(), keys);
        return default(Dictionary<string, T>);
    }

    public async Task<T> GetOrAddAsync(string key, TimeSpan duration, Func<Task<T>> get)
    {
        using var activity = OTel.Application.StartActivity("RedisCache.GetOrAddAsync");

        T item = await GetAsync(key);

        if (item == null)
        {
            item = await get();
            await SetAsync(key, item, duration);
        }

        return item;
    }

    public async Task RemoveAsync(string key)
    {
        using var activity = OTel.Application.StartActivity("RedisCache.RemoveAsync");

        try
        {
            var item = await Db.StringGetAsync(key);

            if (item.HasValue)
            {
                await Db.KeyDeleteAsync(key);
            }
        } catch (Exception ex)
        {
            // don't crash
            _logger.LogError("Unable to retrieve {type} with Key: {key} from Redis Cache becasue of error: {redisConnectionException}.", _type.ToString(), key, ex.Message);
            activity?.RecordException(ex);
        }
    }

    public async Task SetAsync(string key, T item, TimeSpan expiration)
    {
        using var activity = OTel.Application.StartActivity("RedisCache.SetAsync");

        var cacheKey = _type.ToString() + key;

        try
        {
            await Db.StringSetAsync(cacheKey, JsonSerializer.Serialize(item), expiration);
            _logger.LogDebug("Persisted {type} with Key: {key} in Redis Cache succesfullly.", _type.ToString(), key);
        } catch (Exception ex)
        {
            // don't crash the application when the redis cache is not available, just log the error
            _logger.LogError("Unable to persist {type} with key: {key} in Redis Cache because of error", _type.ToString(), key, ex.Message);
            activity?.RecordException(ex);
        }
    }
}
