
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.NodeApiClient;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints;

public static class SelectOrganisationEndpoints
{
    public static void RegisterSelectOrganisationEndpoints(this WebApplication app)
    {
        // Test with Applications
        app.MapGet("select-applications", ([FromKeyedServices(NodeApiName.Applications)] HttpClient client) => {
            return Results.Ok(client.BaseAddress);
        });

        // Test with Directories
        app.MapGet("select-directories", ([FromKeyedServices(NodeApiName.Directories)] HttpClient client) => {
            return Results.Ok(client.BaseAddress);
        });

        // Test using injected IGetClientByServiceId
        app.MapGet("select-injected", async ([FromServices] IInteractor<GetServiceApiSecretByServiceIdRequest, GetServiceApiSecretByServiceIdResponse> service) => {
            var result = await service.InvokeAsync(new GetServiceApiSecretByServiceIdRequest { ServiceId = Guid.Parse("") });
            return Results.Json(result);
        });
    }
}
