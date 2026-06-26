# Logging Strategy and Guidelines

This document outlines the logging patterns, architecture, and cost-control guidelines implemented for the DfE Sign-In (DSI) .NET platform.

---

## 1. Core Principles

Our logging strategy is built on standard .NET core abstractions to ensure unified diagnostic tracking across all microservices and web frontends:
*   **Structured Logging**: All logs are written with message templates (e.g. `logger.LogInformation("Processing user {UserId}", userId)`) so properties are preserved as searchable, structured fields in query tools.
*   **Decoupled Telemetry**: Code relies solely on standard Microsoft logging abstractions (`ILogger<T>`). Exporting logs to consoles, OTLP collectors, or Azure Monitor is handled at runtime via configuration.
*   **Correlation & Tracing**: Every request is correlated across microservices using OpenTelemetry's built-in W3C Trace Context, alongside optional user-provided correlation IDs from request headers.

---

## 2. Standard Logging Patterns

### Standard Dependency Injection
Inject the generic `ILogger<T>` interface directly via the constructor of non-static classes:

```csharp
public sealed class ChangeJobTitleController(
    IInteractionDispatcher interaction,
    ILogger<ChangeJobTitleController> logger
) : Controller
{
    [HttpPost]
    public async Task<IActionResult> PostIndex(ChangeJobTitleViewModel viewModel)
    {
        logger.LogInformation("User {UserId} requesting job title change", this.User.GetUserId());
        // ...
    }
}
```

### Static Classes & Minimal API Endpoints
C# does not allow static classes to be generic type arguments (e.g. `ILogger<StaticClass>` fails to compile with `CS0718`). To resolve this in static classes and Minimal API endpoints:
1.  Define a nested public marker class named `LogContext` inside the static class.
2.  Use `ILogger<LogContext>` as the parameter type.

```csharp
public static class GetServiceUsersEndpoint
{
    // Public nested class used for logger generic typing
    public sealed class LogContext {}

    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        ILogger<LogContext> logger,
        [AsParameters] GetServiceUsersQuery query)
    {
        logger.LogInformation("Client {ClientId} requesting service users", clientSession.ClientId);
        // ...
    }
}
```

> [!NOTE]
> Making the `LogContext` nested class `public` is required to allow unit test projects to access the type and configure appropriate mocks.

---

## 3. Correlation Tracking

We distinguish between two types of correlation identifiers:

### A. OpenTelemetry Trace IDs (Automatic)
OpenTelemetry automatically manages transaction context propagation. 
*   **TraceId** (`operation_Id` in Azure Application Insights) is generated at the system entry point and automatically carried across HTTP boundaries.
*   No endpoint code or developer instrumentation is required to extract or print the TraceId. It is automatically attached to every log record within the execution context.

### B. Client Correlation IDs (Optional)
API consumers may pass a custom client-specified ID via the `x-correlation-id` HTTP header. 
*   This is captured and placed into the log scope centrally via the **`LogContextEnrichmentMiddleware`**.
*   It is registered in `Program.cs` right after the cancellation middleware:
    ```csharp
    app.UseMiddleware<CancellationContextMiddleware>();
    app.UseMiddleware<Dfe.SignIn.WebFramework.LogContextEnrichmentMiddleware>();
    ```
*   When present, the property `ClientCorrelationId` is automatically appended as structured metadata to every log message generated during that HTTP request.

### C. Session Identity Context (Automatic — via `LogContextEnrichmentMiddleware`)

The middleware also enriches the log scope with request identity context so that every log
entry for a request is automatically linked to the user that made it. No endpoint
code is required.

| Application Type | Scope Property | Source | Value |
|---|---|---|---|
| Web apps (OIDC-authenticated) | `UserId` | `ClaimTypes.NameIdentifier` claim | Authenticated user's GUID |
| Public API | — | Endpoints manually include client details | Logged explicitly in handlers |
| Internal API | — | Service-to-service only | Not enriched (OTel TraceId is sufficient) |

> [!WARNING]
> **Data Protection**: `UserId` values are persistent pseudonymous identifiers and are
> considered **personal data** under GDPR. Access to log storage (Azure Monitor / Log
> Analytics workspaces) **must** be restricted to authorised operations staff only.
> Ensure your Data Protection Impact Assessment (DPIA) covers the logging of user identifiers.
> User identifiers must never be logged alongside other personal data fields (e.g. name, email)
> in the same log statement.

#### How it works

For **web apps**, the middleware reads the authenticated user's identity directly from
`context.User` (populated by OIDC authentication which runs before this middleware):

```csharp
// Automatically available in log scope for all authenticated web requests
// { UserId: "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

For **APIs** (Public API / Internal API), the client or service context is not enriched in
the middleware scope. Instead, log messages within endpoints should include the client context explicitly:

```csharp
logger.LogInformation("Processing request for client {ClientId}", clientSession.ClientId);
```

You can query the User ID property in Application Insights / Log Analytics:

```kusto
traces
| where customDimensions.UserId == "3fa85f64-5717-4562-b3fc-2c963f66afa6"
| order by timestamp desc
```

---

## 4. Cost Optimization & Sampling (Azure Monitor)

Logging telemetry is a major driver of cloud hosting costs. To prevent log ballooning when exporting to Azure Monitor (Application Insights):

### Dynamic Adaptive Sampling
Azure Monitor integration is enabled via `ServiceDefaults` if `APPLICATIONINSIGHTS_CONNECTION_STRING` is configured.
*   **Exceptions and Error Logs are always sent (100% ingestion)** to ensure troubleshooting capability.
*   **Successful Traces and Dependency Calls are sampled** based on the configured ratio.

### Configuration Configuration
The sampling ratio is configurable dynamically and defaults to a highly cost-conscious **10%** if not explicitly set:

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options => {
        // Defaults to 0.1 (10% of telemetry traces) if config is missing.
        options.SamplingRatio =
            float.TryParse(builder.Configuration["AzureMonitor:SamplingRatio"], out var ratio)
                ? ratio
                : 0.1f;
    });
```

*   **Production appsettings (`appsettings.json`)**:
    ```json
    "AzureMonitor": {
      "SamplingRatio": "1.0" // Adjust based on required visibility/traffic volume (e.g. 0.1 to 1.0)
    }
    ```

---

## 5. Log Level Environment Guidelines

To ensure developer productivity without flooding production logs, follow these log level conventions:

### Production Settings (`appsettings.json`)
Quiet by default. Focuses on system warnings and application-specific info level tracking.
```json
"Logging": {
  "LogLevel": {
    "Default": "Warning",
    "Dfe.SignIn": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore": "Warning"
  }
}
```

### Local/Dev Settings (`appsettings.Development.json`)
Verbose developer diagnostics including database queries.
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Dfe.SignIn": "Debug",
    "Microsoft.AspNetCore": "Information",
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```
