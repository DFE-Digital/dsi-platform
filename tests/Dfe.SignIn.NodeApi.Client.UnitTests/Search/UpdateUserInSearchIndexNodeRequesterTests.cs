using System.Net;
using Dfe.SignIn.Core.Contracts.Search;
using Dfe.SignIn.NodeApi.Client.Search;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Search;

[TestClass]
public sealed class UpdateUserInSearchIndexNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            UpdateUserInSearchIndexRequest,
            UpdateUserInSearchIndexNodeRequester
        >();
    }

    private static UpdateUserInSearchIndexNodeRequester CreateUpdateUserInSearchIndexNodeRequester(
        Dictionary<string, MappedResponse> responseMappings,
        List<string>? capturedLogs = null)
    {
        var searchHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var searchClient = new HttpClient(searchHandlerMock.Object) {
            BaseAddress = new Uri("http://search.localhost")
        };

        capturedLogs ??= [];
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<UpdateUserInSearchIndexNodeRequester>(capturedLogs.Add);

        return new UpdateUserInSearchIndexNodeRequester(searchClient, mockLogger.Object);
    }

    [TestMethod]
    public async Task LogsSuccess_WhenSearchIndexSuccessful()
    {
        var capturedLogs = new List<string>();
        var responseMapping = new Dictionary<string, MappedResponse>() {
            ["(POST) http://search.localhost/users/update-index"] =
                new MappedResponse(HttpStatusCode.OK),
        };

        var userId = Guid.NewGuid();
        var interactor = CreateUpdateUserInSearchIndexNodeRequester(responseMapping, capturedLogs);

        await interactor.InvokeAsync(new UpdateUserInSearchIndexRequest { UserId = userId });

        Assert.Contains($"Information: Updated search index for user '{userId}'.", capturedLogs);
    }

    [TestMethod]
    public async Task LogsFailure_WhenSearchIndexNoSuccessful()
    {
        var capturedLogs = new List<string>();
        var responseMapping = new Dictionary<string, MappedResponse>() {
            ["(POST) http://search.localhost/users/update-index"] =
                new MappedResponse(HttpStatusCode.BadRequest),
        };

        var userId = Guid.NewGuid();
        var interactor = CreateUpdateUserInSearchIndexNodeRequester(responseMapping, capturedLogs);

        await interactor.InvokeAsync(new UpdateUserInSearchIndexRequest { UserId = userId });

        Assert.Contains($"Error: Unable to update search index for user '{userId}'.", capturedLogs);
    }
}

