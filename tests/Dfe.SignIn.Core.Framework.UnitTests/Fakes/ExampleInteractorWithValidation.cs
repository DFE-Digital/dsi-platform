using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Core.Framework.UnitTests.Fakes;

public sealed record ExampleInteractorWithValidationRequest
{
    [MinLength(3)]
    public required string Name { get; init; }
}

public sealed record ExampleInteractorWithValidationResponse
{
    [Range(0f, 1f)]
    public required float Percentage { get; init; }
}

public sealed class ExampleInteractorWithValidation_ApiRequester
    : IInteractor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
{
    public Task<ExampleInteractorWithValidationResponse> InvokeAsync(ExampleInteractorWithValidationRequest request)
    {
        return Task.FromResult(new ExampleInteractorWithValidationResponse
        {
            Percentage = 1f,
        });
    }
}
