using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeEntraChangeEmailService : IEntraChangeEmailService
{
    public Func<Guid, string, CancellationToken, Task<Result>>? OnChangeEmail { get; set; }

    public Task<Result> ChangeEmailAsync(Guid externalUserId, string newEmailAddress, CancellationToken cancellationToken = default)
    {
        return this.OnChangeEmail != null
            ? this.OnChangeEmail(externalUserId, newEmailAddress, cancellationToken)
            : Task.FromResult(Result.Success());
    }
}
