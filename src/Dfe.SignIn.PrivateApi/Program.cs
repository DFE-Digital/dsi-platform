using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.PrivateApi.Endpoints;
using Dfe.SignIn.PrivateApi.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer(); // This is required for Swagger
builder.Services.AddSwaggerGen(); // This adds Swagger Generator

// Add Entity Framework DbContext
builder.Services.AddDbContext<DbOrganisationsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();

var app = builder.Build();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My private API V1");
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/greeting", () => "Welcome to my article about Minimal API!");

app.MapGet("users/{userId}/organisationservices/{clientId}", UserOrganisationServices.GetUserOrganisationServices);

app.Run();
