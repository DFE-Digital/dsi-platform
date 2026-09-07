
using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Refit;

public static class RefitTestHelper
{
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

    /// <summary>
    /// Creates a mock IApiResponse representing an error response with an optional ProblemDetails payload.
    /// Used when testing controller handling of RFC 7807 problem details.
    /// </summary>
    public static IApiResponse CreateProblemResponse(HttpStatusCode statusCode, object? problemDetailsContent = null)
    {
        ApiException? apiException = null;

        if (problemDetailsContent != null) {
            var json = JsonSerializer.Serialize(problemDetailsContent);
            var httpResponse = new HttpResponseMessage(statusCode) {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            apiException = ApiException.Create(
                new HttpRequestMessage(HttpMethod.Post, "https://internal-api"),
                HttpMethod.Post,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        var mock = new Mock<IApiResponse>();
        mock.SetupGet(r => r.IsSuccessStatusCode).Returns(false);
        mock.SetupGet(r => r.StatusCode).Returns(statusCode);
        mock.SetupGet(r => r.Error).Returns(apiException);

        return mock.Object;
    }
}
