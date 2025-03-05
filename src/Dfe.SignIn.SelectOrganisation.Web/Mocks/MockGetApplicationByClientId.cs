using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications.Interactions;

public sealed class MockGetApplicationByClientId
    : IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse>
{
    public Task<GetApplicationByClientIdResponse> InvokeAsync(GetApplicationByClientIdRequest request)
    {
        return Task.FromResult(new GetApplicationByClientIdResponse {
            Application = new() {
                Id = new Guid("798d1635-66e2-40c2-9917-bb45593133c4"),
                ClientId = request.ClientId,
                Name = "Mock Service",
                Description = "A mocked application.",
                ServiceHomeUrl = new Uri("https://mock.service.localhost/"),
                IsExternalService = true,
                IsHiddenService = false,
                IsIdOnlyService = false,
            },
        });
    }
}
