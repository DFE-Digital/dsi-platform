namespace Dfe.SignIn.PublicApi.Endpoints;

public static class SelectOrganisationEndpoints
{
    public static void RegisterSelectOrganisationEndpoints(this WebApplication app)
    {
        app.MapPost("select-organisation", () => {
            return 0;
        });
    }
}
