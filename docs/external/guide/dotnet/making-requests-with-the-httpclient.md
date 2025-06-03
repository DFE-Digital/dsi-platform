# Making requests with the HttpClient

The [](xref:Dfe.SignIn.PublicApi.Client.IPublicApiClient) service can be used to access a [](xref:System.Net.Http.HttpClient) which includes an `Authorization` bearer token and is configured with the base address of the DfE Sign-in API.

## Example: Get user access to service

The following code snippet demonstrates how to invoke the [Get user access to service](https://github.com/DFE-Digital/login.dfe.public-api?tab=readme-ov-file#get-user-access-to-service) API:

```csharp
public sealed class CustomApplicationClass(IPublicApiClient api)
{
    public async Task DoSomething(
        string serviceId, string organisationId, string userId)
    {
        ...

        var client = api.HttpClient;
        var response = await client.GetFromJsonAsync<UserAccessToService>(
            $"services/{serviceId}/organisations/{organisationId}/{userId}"
        );

        ...
    }
}
```
