using Microsoft.EntityFrameworkCore;

namespace MrJB.IDS.Cache.EntityFramework;

public class MigrationsDbContext : ApplicationDbContext
{
    public MigrationsDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // override connection string
        string connectionString = "Persist Security Info=False;User ID=sa;Password=im938eKAW3K7qM0GZ;Initial Catalog=mrjb-identityserver;Server=127.0.0.1;Encrypt=True;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);

        //Debugger.Launch();
    }
}