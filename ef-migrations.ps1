clear;

$command=$args[0]

dotnet ef -c MigrationsDbContext $command
