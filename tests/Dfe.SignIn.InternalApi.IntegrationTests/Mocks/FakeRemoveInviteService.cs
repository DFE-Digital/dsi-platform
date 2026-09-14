using Dfe.SignIn.InternalApi.Services.Search;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Mocks;

public sealed class FakeRemoveInviteService : IRemoveInviteService
{
    private readonly Dictionary<Guid, List<Guid>> RemovedInvitationsByUserId = [];

    public Task Handle(Guid userId, Guid invitationId)
    {
        if (this.RemovedInvitationsByUserId.ContainsKey(userId)) {
            this.RemovedInvitationsByUserId[userId].Add(invitationId);
        }
        this.RemovedInvitationsByUserId.Add(userId, [invitationId]);

        return Task.CompletedTask;
    }
}
