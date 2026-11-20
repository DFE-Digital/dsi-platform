using Dfe.SignIn.InternalApi.Infrastructure.Search;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeInvitationSearchRemover : IInvitationSearchRemover
{
    private readonly Dictionary<Guid, List<Guid>> RemovedInvitationsByUserId = [];

    public Task Handle(Guid userId, Guid invitationId)
    {
        if (this.RemovedInvitationsByUserId.TryGetValue(userId, out var removedInvitations)) {
            removedInvitations.Add(invitationId);
            return Task.CompletedTask;
        }

        this.RemovedInvitationsByUserId.Add(userId, [invitationId]);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        this.RemovedInvitationsByUserId.Clear();
    }
}
