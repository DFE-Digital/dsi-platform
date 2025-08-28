# Using interactions

Some of the functionality provided by [](xref:Dfe.SignIn.PublicApi.Client) is provided via the [](xref:Dfe.SignIn.Core.Framework.IInteractor`1) abstraction.

## Example: Create a "select organisation" request

The [IInteractionDispatcher](xref:Dfe.SignIn.Core.Framework.IInteractionDispatcher) service can be injected into an application class.

An interaction can be dispatched asynchronously with a request model which will then yield a response model:

```csharp
public sealed class CustomApplicationClass(
    IInteractionDispatcher interaction
) {
    public async Task DoSomething()
    {
        var response = await interaction.DispatchAsync(
            new CreateSelectOrganisationSession_PublicApiRequest {
                UserId = /* some user ID */,
                CallbackUrl = /* some callback URL */,
            }
        ).To<CreateSelectOrganisationSession_PublicApiResponse>();
    }
}
```

## Example: Mocking a "select organisation" request

Interactors can be mocked with the likes of [Moq](https://github.com/devlooped/moq) and [Moq.AutoMocker](https://github.com/moq/Moq.AutoMocker):

```csharp
var fakeResponse = new CreateSelectOrganisationSession_PublicApiResponse {
    RequestId = new Guid("78f29d5a-ca9d-4605-96ea-b3ef789131d2"),
    HasOptions = false,
    Url = new Uri("https://select-organisation.localhost"),
};

autoMocker.GetMock<IInteractionDispatcher>()
    .Setup(x => x.DispatchAsync(
        It.IsAny<InteractionContext<CreateSelectOrganisationSession_PublicApiRequest>>(),
        It.IsAny<CancellationToken>()
    ))
    .ReturnsAsync(fakeResponse);
```

## Validation exceptions

When dispatched any data annotation attributes in the request model of an interaction are validated automatically by the dispatch mechanism. Any validation results are recorded into the invocation context which can be accessed and amended by the interactor.

Interactor implementations can record any additional request validation results using the provided [](xref:Dfe.SignIn.Core.Framework.InteractionContext`1) object:

```csharp
context.AddValidationError("Specify a valid foo", nameof(ExampleRequest.Foo));
```

An interactor implementation should throw an exception when there are validation errors:

```csharp
context.ThrowIfHasValidationErrors();
```

For performance reasons an interactor might decide to have multiple stages of request validation:

```csharp
// Covers data annotations and basic validation checks.
if (context.Request.Foo == 1 && context.Request.Bar == 2) {
    context.AddValidationError("Cannot be 2 when foo is 1", nameof(ExampleRequest.Bar));
}
context.ThrowIfHasValidationErrors();

// Requires more complex processing.
var slowerResult = await this.SlowAsync(context.Request);
if (slowerResult is null) {
    context.AddValidationError("There is no record for bar", nameof(ExampleRequest.Bar));
}
context.ThrowIfHasValidationErrors();
```

There are two types of validation exception:

- [](xref:Dfe.SignIn.Core.Framework.InvalidRequestException) occurs when a request model was invalid.

- [](xref:Dfe.SignIn.Core.Framework.InvalidResponseException) occurs when a response model was invalid.

## Interaction exceptions

A specialised [](xref:Dfe.SignIn.Core.Framework.InteractionException) exception is thrown where applicable. Such scenarios are documented on the request model type.
