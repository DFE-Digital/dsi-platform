using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.SupportTickets;

/// <summary>
/// Use case for getting a list of application names that can be chosen from
/// when raising a support ticket.
/// </summary>
public sealed class GetApplicationNamesForSupportTicketUseCase(
    IUnitOfWorkOrganisations unitOfWork
) : Interactor<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationNamesForSupportTicketResponse> InvokeAsync(
        InteractionContext<GetApplicationNamesForSupportTicketRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var applications = await unitOfWork.Repository<ServiceEntity>()
                    .Where(x => !x.IsChildService && !x.ServiceParams.Any(sp =>
                        sp.ParamName == "helpHidden" &&
                        sp.ParamValue == "true"))
                    .Select(x => new ApplicationNameForSupportTicket { Name = x.Name })
                    .OrderBy(x => x.Name)
                    .ToListAsync(cancellationToken: cancellationToken);

        applications.Add(new ApplicationNameForSupportTicket { Name = "Other (please specify)" });
        applications.Add(new ApplicationNameForSupportTicket { Name = "None" });

        return new GetApplicationNamesForSupportTicketResponse {
            Applications = applications
        };
    }
}
