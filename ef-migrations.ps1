clear;

$command=$args[0]

Write-Host  "dotnet ef $command -c MigrationsDbContext --project MrJB.IDS.Cache"
dotnet ef $command -c MigrationsDbContext --project MrJB.IDS.Cache
