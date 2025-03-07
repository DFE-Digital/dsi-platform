using System.Text.Json.Serialization;
using Dfe.SignIn.PublicApi.Endpoints;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
});

// Add services to the container.

// https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2293
builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(
        namingPolicy: JsonNamingPolicy.CamelCase
    ));
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(
        namingPolicy: JsonNamingPolicy.CamelCase
    ));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => {
    static string GetXmlFileName(Type type)
    {
        return type.Assembly.Location.Replace(".dll", ".xml").Replace(".exe", ".xml");
    }
    config.UseInlineDefinitionsForEnums();

    // Include XML comments for 'SignIn.Core.Models.dll' assembly.
    config.IncludeXmlComments(GetXmlFileName(typeof(Dfe.SignIn.Core.Models.Organisations.OrganisationModel)));
    // Include XML comments for 'SignIn.Core.PublicModels.dll' assembly.
    config.IncludeXmlComments(GetXmlFileName(typeof(Dfe.SignIn.Core.PublicModels.SelectOrganisation.OrganisationFilter)));
    // Include XML comments for 'SignIn.Core.PublicApi.dll' assembly.
    config.IncludeXmlComments(GetXmlFileName(typeof(Program)));
});

builder.Services.AddAutoMapper(options => {
    // Add mapping profiles here...
    //options.AddProfile<ExampleMappingProfile>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterSelectOrganisationEndpoints();

app.Run();
