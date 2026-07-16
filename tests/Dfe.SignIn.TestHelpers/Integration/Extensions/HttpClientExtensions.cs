namespace Dfe.SignIn.TestHelpers.Integration.Extensions;

public static class HttpClientExtensions
{
    public static HttpClient WithAuthentication(this HttpClient client, string? userId = null, string? userName = null, params string[] roles)
    {
        client.DefaultRequestHeaders.Add(TestAuthHandler.EnableAuthHeaderName, bool.TrueString);

        if (!string.IsNullOrWhiteSpace(userId)) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId);
        }

        if (!string.IsNullOrWhiteSpace(userName)) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeaderName, userName);
        }

        foreach (var role in roles.Where(static r => !string.IsNullOrWhiteSpace(r))) {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        }

        return client;
    }
}
