clear;

$command=$args[0]

Write-Host  "dotnet ef $command -c MigrationsDbContext"
dotnet ef $command -c MigrationsDbContext
