using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace MrJB.IDS.Cache.EntityFramework;

public class MigrationsDbContext : ApplicationDbContext
{
    public MigrationsDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
            .AddJsonFile("appsettings.migrations.json")
            .AddEnvironmentVariables()
            .Build();

        //var optionsBuilder = new DbContextOptionsBuilder<ConfigurationDbContext>();
        //var storeOptions = new ConfigurationStoreOptions();

        //optionsBuilder.UseSqlServer(config["ConnectionString"], sqlServerOptionsAction: o => o.MigrationsAssembly("Identity.API"));

        //return new ConfigurationDbContext(optionsBuilder.Options, storeOptions);

        // override connection string
        string connectionString = "Persist Security Info=False;User ID=sa;Password=im938eKAW3K7qM0GZ;Initial Catalog=mrjb-identityserver;Server=127.0.0.1;Encrypt=True;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        Debugger.Launch();
    }
}