using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;

namespace Dfe.SignIn.PublicApi.Fakes;

/// <summary>
/// ...
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class FakeFilterOrganisationsForUser
    : IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse>
{
    /// <inheritdoc/>
    public Task<FilterOrganisationsForUserResponse> InvokeAsync(
        FilterOrganisationsForUserRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<FilterOrganisationsForUserResponse>(new() {
            FilteredOrganisations = [
                new() {
                    Id = new Guid("a4412d34-6471-4663-8d70-73fe6617b5e5"),
                    Name = "Organisation A",
                    LegalName = "Legal name A",
                    Status = 0,
                },
                new() {
                    Id = new Guid("561cdabf-d2f8-48f3-a66b-0f943837c9d7"),
                    Name = "Organisation B",
                    LegalName = "Legal name B",
                    Status = 0,
                },
            ],
        });
    }
}
