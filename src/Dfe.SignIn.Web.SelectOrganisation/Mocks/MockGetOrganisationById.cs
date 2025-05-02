// using Dfe.SignIn.Core.Framework;
// using Dfe.SignIn.Core.InternalModels.Organisations.Interactions;

// namespace Dfe.SignIn.Web.SelectOrganisation.Mocks;

// public sealed class MockGetOrganisationById
//     : IInteractor<GetOrganisationByIdRequest, GetOrganisationByIdResponse>
// {
//     public Task<GetOrganisationByIdResponse> InvokeAsync(GetOrganisationByIdRequest request)
//     {
//         MockOrganisations.Models.TryGetValue(request.OrganisationId, out var model);
//         return Task.FromResult(new GetOrganisationByIdResponse {
//             Organisation = model,
//         });
//     }
// }
