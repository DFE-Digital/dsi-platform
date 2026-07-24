using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using Refit;

namespace Dfe.SignIn.TestHelpers;

public static class InternalApiAutoMockerExtensions
{
    public static async Task MockRefitValidationError<TClient>(
        this AutoMocker autoMocker,
        Expression<Func<TClient, Task>> methodExpression,
        string modelStateKey,
        string errorMessage = "Invalid value")
        where TClient : class
    {
        var ex = await CreateBadRequestApiException(modelStateKey, errorMessage);

        autoMocker.GetMock<TClient>()
            .Setup(methodExpression)
            .ThrowsAsync(ex);
    }

    private static async Task<ApiException> CreateBadRequestApiException(
        string modelStateKey,
        string errorMessage)
    {
        var payload = new ValidationProblemDetails {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Errors = new Dictionary<string, string[]> {
                [modelStateKey] = [errorMessage],
            },
        };

        var json = JsonSerializer.Serialize(payload);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://unit-test.local/change-name");
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
            RequestMessage = request,
            Content = new StringContent(json, Encoding.UTF8, "application/problem+json"),
        };

        return await ApiException.Create(request, request.Method, response, new RefitSettings());
    }
}
