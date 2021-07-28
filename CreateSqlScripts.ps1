

[CmdletBinding()]
param (
    [Parameter()]
    [string]$Context,

    [Parameter()]
    [string]$Name
)

$Name = "$($Name)_$((get-date).ToString("yyyy-MM-ddTHH-mm-ss"))"

if ($Context -eq "App") {
    
    dotnet ef migrations script `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.Sqlite `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration sqlite `
        --output "DbScripts\AppDb_Sqlite_$($Name).sql"

    dotnet ef migrations script `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.Postgres `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration postgres `
        --output "DbScripts\AppDb_Postgres_$($Name).sql" `
        --idempotent

    dotnet ef migrations script `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.SqlServer `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration sqlserver `
        --output "DbScripts\AppDb_SqlServer_$($Name).sql" `
        --idempotent
}

if ($Context -eq "Auth") {
    
    dotnet ef migrations script `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.Sqlite `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration sqlite `
        --output "DbScripts\AuthDb_Sqlite_$($Name).sql" `

    dotnet ef migrations script `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.Postgres `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration postgres `
        --output "DbScripts\AuthDb_Postgres_$($Name).sql" `
        --idempotent

    dotnet ef migrations script `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.SqlServer `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration sqlserver `
        --output "DbScripts\AuthDb_SqlServer_$($Name).sql" `
        --idempotent
}