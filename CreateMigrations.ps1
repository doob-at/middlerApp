

[CmdletBinding()]
param (
    [Parameter()]
    [string]$Context,

    [Parameter()]
    [string]$Name
)


$Name = "$($Name)_$((get-date).ToString("yyyy-MM-ddTHH-mm-ss"))"

if ($Context -eq "App") {
    
    dotnet ef migrations add $Name `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.Sqlite `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration sqlite

    dotnet ef migrations add $Name `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.Postgres `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration postgres

    dotnet ef migrations add $Name `
        --context AppDbContext `
        --project source\DataAccess\middlerApp.DataAccess.SqlServer `
        --startup-project source\DataAccess\middlerApp.DataAccess.MigrationBuilder `
        --configuration sqlserver
}


if ($Context -eq "Auth") {

    dotnet ef migrations add $Name `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.Sqlite `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration sqlite 

    dotnet ef migrations add $Name `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.Postgres `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration postgres 

    dotnet ef migrations add $Name `
        --context AuthDbContext `
        --project source\Authentication\middlerApp.Auth.SqlServer `
        --startup-project source\Authentication\middlerApp.Auth.MigrationBuilder `
        --configuration sqlserver 
}
