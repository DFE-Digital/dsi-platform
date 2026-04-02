var builder = DistributedApplication.CreateBuilder(args);

var frontend = builder.AddDockerfile("frontend", "../../", "docker/frontend/Dockerfile")
    .WithHttpEndpoint(targetPort: 8080, name: "http");

builder.AddProject<Projects.Dfe_SignIn_Web_Help>("help")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Local")
    .WithEnvironment("Assets__BaseAddress", frontend.GetEndpoint("http"))
    .WithEnvironment("SecurityHeaderPolicy__AllowedOrigins__0", "http://localhost:*")
    .WithEnvironment("SecurityHeaderPolicy__AllowedOrigins__1", "https://localhost:*")
    .WaitFor(frontend);

builder.Build().Run();
