var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("infra-redis")
    .WithDataVolume()
    // Uncomment .WithRedisInsight() for advanced memory analysis, active profilers, and module reporting.
    // .WithRedisInsight()
    // Defaults to Redis Commander for a fast, lightweight cache viewer that uses minimal resources.
    .WithRedisCommander();

var frontend = builder.AddDockerfile("infra-frontend", "../../", "docker/frontend/Dockerfile")
    .WithHttpEndpoint(targetPort: 8080, name: "http");

builder.AddProject<Projects.Dfe_SignIn_Web_Help>("app-help", launchProfileName: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Local")
    .WithEnvironment("InteractionsRedisCache__ConnectionString", redis.GetEndpoint("tcp"))
    .WithEnvironment("Assets__BaseAddress", frontend.GetEndpoint("http"))
    .WaitFor(frontend)
    .WaitFor(redis);

builder.AddProject<Projects.Dfe_SignIn_Web_Profile>("app-profile", launchProfileName: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Local")
    .WithEnvironment("GeneralRedisCache__ConnectionString", redis.GetEndpoint("tcp"))
    .WithEnvironment("SessionRedisCache__ConnectionString", redis.GetEndpoint("tcp"))
    .WithEnvironment("TokenRedisCache__ConnectionString", redis.GetEndpoint("tcp"))
    .WithEnvironment("Assets__BaseAddress", frontend.GetEndpoint("http"))
    .WaitFor(frontend)
    .WaitFor(redis);

builder.AddExecutable("tool-tls-proxy", "pwsh", "../../", "-Command", "Start-DsiTlsProxy");

builder.Build().Run();
