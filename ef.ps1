clear;

$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host  "dotnet ef $args -c MigrationsDbContext --project src\MrJB.IDS.Cache"

dotnet ef $args -c ApplicationDbContext --project src\MrJB.IDS.Cache
