using System.Diagnostics;

namespace MrJB.IDS.Cache;

public static class OTel
{
    public const string ApplicationName = "MRJB.IDS.Cache";

    public static ActivitySource Application = new ActivitySource(ApplicationName);

    public static class Spans
    {
        public const string ClientId = "client.id";

        public const string TenantId = "tenant.id";
    }
}

