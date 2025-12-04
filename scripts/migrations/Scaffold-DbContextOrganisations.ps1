param (
    [string]$ConnectionString = "<connection_string>"
)

dotnet ef dbcontext scaffold `
    "$ConnectionString" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --context "DbOrganisationsContext" `
    --project "../src/Dfe.SignIn.Gateways.EntityFramework/Dfe.SignIn.Gateways.EntityFramework.csproj" `
    --output-dir "../Dfe.SignIn.Core.Entities/Organisations" `
    --namespace "Dfe.SignIn.Core.Entities.Organisations" `
    --context-dir "../Dfe.SignIn.Gateways.EntityFramework" `
    --context-namespace "Dfe.SignIn.Gateways.EntityFramework" `
    --no-onconfiguring
