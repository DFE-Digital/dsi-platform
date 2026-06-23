using Dfe.SignIn.FauApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); // This is required for Swagger
builder.Services.AddSwaggerGen(); // This adds Swagger Generator

// Register HttpClientFactory
builder.Services.AddHttpClient<UserClient>();

var app = builder.Build();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Public API V1");
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/users/{userId}/organisationservices", async (Guid userId, UserClient client) => {
    var result = await client.GetUserOrganisationServicesAsync(userId);
    return result is null ? Results.NotFound() : Results.Ok(result);
});

app.Run();
