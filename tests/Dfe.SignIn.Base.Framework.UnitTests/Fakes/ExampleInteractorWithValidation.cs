using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Base.Framework.UnitTests.Fakes;

public enum ExampleInteractorEnum
{
    FirstValue = 0,
    SecondValue = 1,
}

public sealed record ExampleInteractorWithValidationRequest
{
    [MinLength(3)]
    public required string Name { get; init; }

    [EnumDataType(typeof(ExampleInteractorEnum))]
    public required ExampleInteractorEnum SomeEnumProperty { get; init; }

    [EnumDataType(typeof(ExampleInteractorEnum))]
    public ExampleInteractorEnum? SomeNullableEnumProperty { get; init; } = null;
}

public sealed record ExampleInteractorWithValidationResponse
{
    [Range(0f, 1f)]
    public required float Percentage { get; init; }
}

public sealed class ExampleInteractorWithValidation_ApiRequester
    : Interactor<ExampleInteractorWithValidationRequest, ExampleInteractorWithValidationResponse>
{
    public override Task<ExampleInteractorWithValidationResponse> InvokeAsync(
        InteractionContext<ExampleInteractorWithValidationRequest> context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ExampleInteractorWithValidationResponse {
            Percentage = 1f,
        });
    }
}
