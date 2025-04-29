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

## Validation exceptions

The [](xref:System.ComponentModel.DataAnnotations.ValidationException) exception is thrown if the request model has one or more invalid properties.


## Interaction exceptions

A specialised [](xref:Dfe.SignIn.Core.Framework.InteractionException) exception is thrown where applicable. Such scenarios are documented on the request model type.
