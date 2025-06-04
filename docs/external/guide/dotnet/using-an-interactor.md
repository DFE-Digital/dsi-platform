# Using an interactor

Some of the functionality provided by [](xref:Dfe.SignIn.PublicApi.Client) is provided via the [](xref:Dfe.SignIn.Core.Framework.IInteractor`2) abstraction.

## Example: Create a "select organisation" request

The [IInteractor](xref:Dfe.SignIn.Core.Framework.IInteractor`2)&lt;[](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.CreateSelectOrganisationSession_PublicApiRequest), [](xref:Dfe.SignIn.PublicApi.Client.SelectOrganisation.CreateSelectOrganisationSession_PublicApiResponse)&gt; service can be injected into an application class.

The interactor can be invoked asynchronously with a request model which will then yield a response model:

```csharp
public sealed class CustomApplicationClass(
    IInteractor<
        CreateSelectOrganisationSession_PublicApiRequest,
        CreateSelectOrganisationSession_PublicApiResponse
    > createSelectOrganisationSession
) {
    public async Task DoSomething()
    {
        var response = await createSelectOrganisationSession.InvokeAsync(new() {
            UserId = /* some user ID */,
            CallbackUrl = /* some callback URL */,
        });
    }
}
```

## Example: Mocking a "select organisation" request

Interactors can be mocked with the likes of [Moq](https://github.com/devlooped/moq) and [Moq.AutoMocker](https://github.com/moq/Moq.AutoMocker):

```csharp
var fakeResponse = new CreateSelectOrganisationSession_PublicApiRequest {
    RequestId = new Guid("78f29d5a-ca9d-4605-96ea-b3ef789131d2"),
    HasOptions = false,
    Url = new Uri("https://select-organisation.localhost"),
};

autoMocker
    .GetMock<IInteractor<
        CreateSelectOrganisationSession_PublicApiRequest,
        CreateSelectOrganisationSession_PublicApiResponse
    >>()
    .Setup(mock => mock.InvokeAsync(
        It.IsAny<CreateSelectOrganisationSession_PublicApiRequest>(),
        It.IsAny<CancellationToken>()
    ))
    .ReturnsAsync(fakeResponse);
```


## Validation exceptions

The [](xref:System.ComponentModel.DataAnnotations.ValidationException) exception is thrown if the request model has one or more invalid properties.


## Interaction exceptions

A specialised [](xref:Dfe.SignIn.Core.Framework.InteractionException) exception is thrown where applicable. Such scenarios are documented on the request model type.
