using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Organisations.Interactions;

public sealed class MockGetOrganisationById
    : IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
{
    public Task<GetOrganisationByIdResponse> InvokeAsync(GetOrganisationByIdRequest request)
    {
        MockOrganisations.Models.TryGetValue(request.Id, out var model);
        return Task.FromResult(new GetOrganisationByIdResponse {
            Organisation = model,
        });
    }
}
