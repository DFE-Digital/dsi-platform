param (
    [string]$ConnectionString = "<connection_string>"
)

dotnet ef dbcontext scaffold `
    "$ConnectionString" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --context "DbDirectoriesContext" `
    --project "../../src/Dfe.SignIn.Gateways.EntityFramework/Dfe.SignIn.Gateways.EntityFramework.csproj" `
    --output-dir "../Dfe.SignIn.Core.Entities/Directories" `
    --namespace "Dfe.SignIn.Core.Entities.Directories" `
    --context-dir "../Dfe.SignIn.Gateways.EntityFramework" `
    --context-namespace "Dfe.SignIn.Gateways.EntityFramework" `
    --no-onconfiguring `
    --table invitation `
    --table invitation_callback `
    --table password_history `
    --table user `
    --table user_code `
    --table user_password_history `
    --table user_password_policy `
    --table user_status_change_reasons
