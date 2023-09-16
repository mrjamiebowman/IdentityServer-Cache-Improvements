using Duende.IdentityServer.EntityFramework.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MrJB.IDS.Cache.Configuration;
using MrJB.IDS.Cache.Data;
using MrJB.IDS.Cache.Models;
using Serilog;
using System.Reflection;

namespace MrJB.IDS.Cache;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        // configuration
        var isConfig = new IdentityServerConfiguration();
        builder.Configuration.GetSection(IdentityServerConfiguration.Position).Bind(isConfig);
        builder.Services.AddSingleton<IdentityServerConfiguration>(isConfig);

        // cache config
        var cacheConfig = new CacheConfiguration();
        builder.Configuration.GetSection(CacheConfiguration.Position).Bind(cacheConfig);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
                            options.UseSqlite(isConfig.ConnectionString));
        
        // assembly name
        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

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
                options.ConfigureDbContext = b => b.UseSqlServer(isConfig.ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            })
        .AddOperationalStore(options =>
        {
            // disable automatic token clean up since we have a worker service that does this.
            // Read: https://www.identityserver.com/articles/efficient-cleaning-up-of-the-persisted-grant-table
            options.EnableTokenCleanup = false;
            options.ConfigureDbContext = b => b.UseSqlServer(isConfig.ConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            options.RemoveConsumedTokens = true;
        })
        /* cache configuration */
        .AddClientStoreCache<ClientStore>()
        .AddResourceStoreCache<ResourceStore>()
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
}