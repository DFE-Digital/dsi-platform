using System.Net;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class CompleteAnyPendingInvitationNodeRequesterTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            CompleteAnyPendingInvitationRequest,
            CompleteAnyPendingInvitationNodeRequester
        >();
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(GET) http://directories.localhost/invitations/by-email/jo.bradford@example.com"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "id": "559c27b8-303e-4aff-b485-037a927827cd",
                    "isCompleted": false
                }
                """),
            ["(POST) http://directories.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/create_user"] =
                new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "id": "7c9d8126-fdc9-42f3-bdfc-bbd567b472ff"
                }
                """),

            // Organisations API
            ["(POST) http://organisations.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"] =
                new MappedResponse(HttpStatusCode.OK),

            // Access API
            ["(POST) http://access.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"] =
                new MappedResponse(HttpStatusCode.OK),
            ["(POST) http://access.localhost/invitations/by-email/jo.bradford@example.com"] =
                new MappedResponse(HttpStatusCode.OK),

            // Search API
            ["(DELETE) http://search.localhost/users/inv-559c27b8-303e-4aff-b485-037a927827cd"] =
                new MappedResponse(HttpStatusCode.OK),
            ["(POST) http://search.localhost/users/update-index"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    private static CompleteAnyPendingInvitationNodeRequester CreateCompleteAnyPendingInvitationNodeRequester(
        Dictionary<string, MappedResponse> responseMappings,
        List<string>? capturedLogs = null)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        var organisationsHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var organisationsClient = new HttpClient(organisationsHandlerMock.Object) {
            BaseAddress = new Uri("http://organisations.localhost")
        };

        var accessHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var accessClient = new HttpClient(accessHandlerMock.Object) {
            BaseAddress = new Uri("http://access.localhost")
        };

        var searchHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var searchClient = new HttpClient(searchHandlerMock.Object) {
            BaseAddress = new Uri("http://search.localhost")
        };

        capturedLogs ??= [];
        var mockLogger = LoggerMocking.GetMockToCaptureLogs<CompleteAnyPendingInvitationNodeRequester>(capturedLogs.Add);

        return new CompleteAnyPendingInvitationNodeRequester(
            directoriesClient, organisationsClient, accessClient, searchClient, mockLogger.Object);
    }

    [TestMethod]
    public async Task InvokeAsync_ResolvesAsNotCompleted_WhenInvitationDoesNotExist()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(GET) http://directories.localhost/invitations/by-email/jo.bradford@example.com"] =
            new MappedResponse(HttpStatusCode.NotFound);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        var response = await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(GET) http://directories.localhost/invitations/by-email/jo.bradford@example.com"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.WasCompleted);
        Assert.IsNull(response.UserId);
    }

    [TestMethod]
    public async Task InvokeAsync_ResolvesAsNotCompleted_WhenInvitationWasAlreadyCompleted()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(GET) http://directories.localhost/invitations/by-email/jo.bradford@example.com"] =
            new MappedResponse(HttpStatusCode.OK, /*lang=json,strict*/ """
                {
                    "id": "559c27b8-303e-4aff-b485-037a927827cd",
                    "isCompleted": true
                }
                """);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        var response = await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(GET) http://directories.localhost/invitations/by-email/jo.bradford@example.com"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.IsNotNull(response);
        Assert.IsFalse(response.WasCompleted);
        Assert.IsNull(response.UserId);
    }

    [TestMethod]
    public async Task InvokeAsync_MakesRequestToConvertInvitationToUser()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(POST) http://directories.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/create_user"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Information: Fulfilled pending invitation '559c27b8-303e-4aff-b485-037a927827cd' for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenRequestToConvertInvitationToUserWasNotSuccessful()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://directories.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/create_user"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
                EmailAddress = "jo.bradford@example.com",
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            }));
    }

    [TestMethod]
    public async Task InvokeAsync_MakesRequestToAssignOrganisationsFromInvitation()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(POST) http://organisations.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Information: Assigned organisations and services from pending invitation '559c27b8-303e-4aff-b485-037a927827cd' for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenRequestToAssignOrganisationsFromInvitationWasNotSuccessful()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://organisations.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
                EmailAddress = "jo.bradford@example.com",
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            }));
    }

    [TestMethod]
    public async Task InvokeAsync_MakesRequestToAssignServicesFromInvitation()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(POST) http://access.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Information: Assigned organisations and services from pending invitation '559c27b8-303e-4aff-b485-037a927827cd' for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);
    }

    [TestMethod]
    public async Task InvokeAsync_Throws_WhenRequestToAssignServicesFromInvitationWasNotSuccessful()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://access.localhost/invitations/559c27b8-303e-4aff-b485-037a927827cd/migrate-to-user"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
                EmailAddress = "jo.bradford@example.com",
                EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
            }));
    }

    [TestMethod]
    public async Task InvokeAsync_MakesRequestToRemoveInvitationFromSearchIndex()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(DELETE) http://search.localhost/users/inv-559c27b8-303e-4aff-b485-037a927827cd"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Information: Removed 'inv-559c27b8-303e-4aff-b485-037a927827cd' from search index for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);
    }

    [TestMethod]
    public async Task InvokeAsync_MakesRequestToUpdateUserInSearchIndex()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(POST) http://search.localhost/users/update-index"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Information: Updated search index pending invitation '559c27b8-303e-4aff-b485-037a927827cd' for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedResponse_WhenInvitationWasCompletedSuccessfully()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        var response = await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.WasCompleted);
        Assert.AreEqual(new Guid("7c9d8126-fdc9-42f3-bdfc-bbd567b472ff"), response.UserId);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedResponseAndLogsFailure_WhenFailedToRemoveInvitationFromSearchIndex()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(DELETE) http://search.localhost/users/inv-559c27b8-303e-4aff-b485-037a927827cd"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        var response = await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(DELETE) http://search.localhost/users/inv-559c27b8-303e-4aff-b485-037a927827cd"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Error: Unable to remove 'inv-559c27b8-303e-4aff-b485-037a927827cd' from search index for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.WasCompleted);
        Assert.AreEqual(new Guid("7c9d8126-fdc9-42f3-bdfc-bbd567b472ff"), response.UserId);
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedResponseAndLogsFailure_WhenFailedToUpdateUserInSearchIndex()
    {
        var responseMappings = GetNodeResponseMappingsForHappyPath();

        responseMappings["(POST) http://search.localhost/users/update-index"] =
            new MappedResponse(HttpStatusCode.InternalServerError);

        var capturedLogs = new List<string>();
        var interactor = CreateCompleteAnyPendingInvitationNodeRequester(responseMappings, capturedLogs);

        var response = await interactor.InvokeAsync(new CompleteAnyPendingInvitationRequest {
            EmailAddress = "jo.bradford@example.com",
            EntraUserId = new Guid("207ec104-8569-4d80-9d16-5f7e1516ae01"),
        });

        var mapping = responseMappings["(POST) http://search.localhost/users/update-index"];
        Assert.AreEqual(1, mapping.InvocationCount);

        Assert.Contains("Error: Unable to update search index pending invitation '559c27b8-303e-4aff-b485-037a927827cd' for user '7c9d8126-fdc9-42f3-bdfc-bbd567b472ff'.", capturedLogs);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.WasCompleted);
        Assert.AreEqual(new Guid("7c9d8126-fdc9-42f3-bdfc-bbd567b472ff"), response.UserId);
    }
}
