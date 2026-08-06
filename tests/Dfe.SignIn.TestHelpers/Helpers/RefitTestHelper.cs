
using System.Net;
using System.Text;
using System.Text.Json;
using Refit;

public static class RefitTestHelper
{
    public static async Task<ApiException> Exception(HttpStatusCode status, string content = "")
    {
        return await ApiException.Create(
                new HttpRequestMessage(HttpMethod.Post, "http://localhost"),
                HttpMethod.Post,
                new HttpResponseMessage(status) { Content = new StringContent(content) },
                new RefitSettings()
            );
    }

    public static async Task<ValidationApiException> ValidationException(HttpStatusCode status, string message = "")
    {
        // 1. Format as a JSON Object matching ProblemDetails structure
        var problemDetails = new {
            title = "One or more validation errors occurred.",
            status = (int)status,
            detail = message
        };

        var json = JsonSerializer.Serialize(problemDetails);

        // 2. Create HttpResponseMessage with application/json header
        var response = new HttpResponseMessage(status) {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "http://localhost"),
            HttpMethod.Post,
            response,
            new RefitSettings()
        );

        // 3. ValidationApiException will now parse ProblemDetails without throwing
        return ValidationApiException.Create(apiException);
    }
}
