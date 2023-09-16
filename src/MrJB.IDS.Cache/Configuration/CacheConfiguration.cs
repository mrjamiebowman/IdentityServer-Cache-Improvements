namespace MrJB.IDS.Cache.Configuration;

public class CacheConfiguration
{
    public const string Position = "Cache";

    public string ConnectionString { get; set; } = "";

    public string InstanceName { get; set; }

    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public TimeSpan SlidingExpiration { get; set; }
}

