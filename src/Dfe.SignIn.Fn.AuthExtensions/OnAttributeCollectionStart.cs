using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Dfe.SignIn.Fn.AuthExtensions;

public class OnAttributeCollectionStart(ILogger<OnAttributeCollectionStart> logger)
{
    [Function("OnAttributeCollectionStart")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest request)
    {
        var requestBody = await request.ReadFromJsonAsync<JsonElement?>()
            ?? throw new InvalidOperationException("Missing request body.");

        logger.LogInformation("C# HTTP trigger function processed a request.");

        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
