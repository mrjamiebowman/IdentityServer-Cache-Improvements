namespace MrJB.IDS.Cache;

public interface IRedisCacheService
{
    Task<T> GetAsync<T>(string key);

    Task<T> GetAsync<T>(List<string> keys);

    Task RemoveAsync(string key);

    Task SetAsync<T>(string key, T value);
}
