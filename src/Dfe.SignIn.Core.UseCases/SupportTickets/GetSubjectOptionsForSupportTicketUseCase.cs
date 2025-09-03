using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;

namespace Dfe.SignIn.Core.UseCases.SupportTickets;

/// <summary>
/// Use case for getting the list of possible subject options when a user is raising a
/// support request.
/// </summary>
public sealed class GetSubjectOptionsForSupportTicketUseCase
    : Interactor<GetSubjectOptionsForSupportTicketRequest, GetSubjectOptionsForSupportTicketResponse>
{
    /// <inheritdoc/>
    public override Task<GetSubjectOptionsForSupportTicketResponse> InvokeAsync(
        InteractionContext<GetSubjectOptionsForSupportTicketRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        return Task.FromResult(new GetSubjectOptionsForSupportTicketResponse {
            SubjectOptions = [
                new() { Code = "create-account", Description = "Setting up a DfE Sign-in account" },
                new() { Code = "service-access", Description = "Using a DfE service" },
                new() { Code = "email-password", Description = "Changing my email or password" },
                new() { Code = "deactivate-account", Description = "Deactivating my account" },
                new() { Code = "approver", Description = "Managing my users as an approver" },
                new() { Code = "add-org", Description = "Adding organisations to your account" },
                new() { Code = "other", Description = "Other (please specify)" },
            ],
        });
    }
}
