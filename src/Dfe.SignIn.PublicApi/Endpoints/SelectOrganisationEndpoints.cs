
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
        app.MapGet("select-injected", async ([FromServices] IInteractor<GetApplicationApiSecretByClientIdRequest, GetApplicationApiSecretByClientIdResponse> service) => {
            var result = await service.InvokeAsync(new GetApplicationApiSecretByClientIdRequest { ClientId = "" });
            return Results.Json(result);
        });
    }
}
