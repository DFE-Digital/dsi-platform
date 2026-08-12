using Dfe.SignIn.Core.Interfaces.ExternalAuth;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeExternalAuthService : IExternalAuthService
{
    public Func<Guid, string, CancellationToken, Task>? OnChangeEmail { get; set; }

    public Task ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken)
    {
        return this.OnChangeEmail != null
            ? this.OnChangeEmail(externalUserId, newEmailAddress, cancellationToken)
            : Task.CompletedTask;
    }
}
