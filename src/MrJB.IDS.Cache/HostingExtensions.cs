using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.Stores;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MrJB.IDS.Cache.Configuration;
using MrJB.IDS.Cache.EntityFramework;
using MrJB.IDS.Cache.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MrJB.IDS.Cache;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        // connection strings
        var aspNetUsersConnectionString = builder.Configuration.GetConnectionString("AspNetUsers");
        var identityServerConnectionString = builder.Configuration.GetConnectionString("IdentityServer");

        // configuration
        var isConfig = new IdentityServerConfiguration();
        builder.Configuration.GetSection(IdentityServerConfiguration.Position).Bind(isConfig);
        builder.Services.AddSingleton<IdentityServerConfiguration>(isConfig);

        // cache config
        var cacheConfig = new CacheConfiguration();
        builder.Configuration.GetSection(CacheConfiguration.Position).Bind(cacheConfig);

        // dbContext: asp.net users
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(aspNetUsersConnectionString));
        
        // assembly name
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // configure caching
        builder.Services.ConfigureCaching(isConfig, cacheConfig);

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;

                // cache settings
                if (cacheConfig.AbsoluteExpirationRelativeToNow.HasValue)
                {
                    options.Caching.ClientStoreExpiration = cacheConfig.AbsoluteExpirationRelativeToNow.Value;
                    options.Caching.ResourceStoreExpiration = cacheConfig.AbsoluteExpirationRelativeToNow.Value;
                }
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(aspNetUsersConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                // disable automatic token clean up since we have a worker service that does this.
                // Read: https://www.identityserver.com/articles/efficient-cleaning-up-of-the-persisted-grant-table
                options.EnableTokenCleanup = false;
                options.ConfigureDbContext = b => b.UseSqlServer(identityServerConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                options.RemoveConsumedTokens = true;
            })
            /* cache configuration */
            .AddClientStoreCache<ClientStore>()
            .AddResourceStoreCache<ResourceStore>()

            //.AddClientStoreCache<YourCustomClientStore>()
            //.AddCorsPolicyCache<YourCustomCorsPolicyService>()
            //.AddResourceStoreCache<YourCustomResourceStore>()
            //.AddIdentityProviderStoreCache<YourCustomAddIdentityProviderStore>()
            ;
        
        builder.Services.AddAuthentication()
            //.AddGoogle(options =>
            //{
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //    // register your IdentityServer with Google at https://console.developers.google.com
            //    // enable the Google+ API
            //    // set the redirect URI to https://localhost:5001/signin-google
            //    options.ClientId = "copy client ID from Google here";
            //    options.ClientSecret = "copy client secret from Google here";
            //})
        ;

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
           .RequireAuthorization();

        return app;
    }

    public static IServiceCollection ConfigureCaching(this IServiceCollection services, IdentityServerConfiguration identityServerConfiguration, CacheConfiguration cacheConfiguration)
    {
        // add injections
        Log.Information("Injecting Redis Cache");

        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        // configure distributed cache using Redis
        services.AddTransient(typeof(ICache<>), typeof(RedisCache<>));

        //// injects IMemoryCache
        //services.AddMemoryCache();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheConfiguration.ConnectionString;
            options.InstanceName = cacheConfiguration.InstanceName;
        });

        var multiplexer = ConnectionMultiplexer.Connect(cacheConfiguration.ConnectionString);
        services.AddSingleton<IConnectionMultiplexer>(multiplexer);

        return services;
    }

    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        // define attributes for your application
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(OTel.ApplicationName, serviceVersion: "1.0.0")
            .AddTelemetrySdk()
            .AddAttributes(new Dictionary<string, object>
            {
                ["host.name"] = Environment.MachineName,
                ["os.description"] = RuntimeInformation.OSDescription,
                ["deployment.environment"] = builder.Environment.EnvironmentName.ToLowerInvariant()
            });
        //.AddConsoleExporter()

        // add open telemetry with azure monitor (application insights)
        builder.Services
            .AddOpenTelemetry()
            .WithTracing(tb => tb
                    .AddSource(OTel.Application.Name)
                    .AddSource(IdentityServerConstants.Tracing.Basic)
                    .AddSource(IdentityServerConstants.Tracing.Cache)
                    .AddSource(IdentityServerConstants.Tracing.Services)
                    .AddSource(IdentityServerConstants.Tracing.Stores)
                    .AddSource(IdentityServerConstants.Tracing.Validation)
                    .ConfigureResource(r => r.AddService(OTel.ApplicationName))
            //.AddAzureMonitorTraceExporter(options => options.ConnectionString = appInsightsConnectionString)
            )
            .WithMetrics(mb => mb.ConfigureResource(r => r.AddService(OTel.ApplicationName)))
            //.AddAzureMonitorMetricExporter(options => options.ConnectionString = appInsightsConnectionString)
            //.AddConsoleExporter()
            ;

        return builder;
    }
}