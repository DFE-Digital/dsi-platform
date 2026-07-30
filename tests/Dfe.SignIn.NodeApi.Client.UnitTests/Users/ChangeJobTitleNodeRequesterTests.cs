using System.Net;
using System.Text.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.NodeApi.Client.Users;
using Moq.AutoMock;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.Users;

[TestClass]
public sealed class ChangeJobTitleNodeRequesterTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            ChangeJobTitleRequest,
            ChangeJobTitleNodeRequester
        >();
    }

    private static ChangeJobTitleNodeRequester CreateChangeJobTitleNodeRequester(
        AutoMocker autoMocker,
        Dictionary<string, MappedResponse> responseMappings)
    {
        var directoriesHandlerMock = HttpClientMocking.GetHandlerWithMappedResponses(responseMappings);
        var directoriesClient = new HttpClient(directoriesHandlerMock.Object) {
            BaseAddress = new Uri("http://directories.localhost")
        };

        return new ChangeJobTitleNodeRequester(
            directoriesClient,
            autoMocker.Get<IInteractionDispatcher>()
        );
    }

    private static Dictionary<string, MappedResponse> GetNodeResponseMappingsForHappyPath()
    {
        return new() {
            // Directories API
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.OK),
        };
    }

    [TestMethod]
    public async Task MakesExpectedRequestToChangeJobTitle()
    {
        var autoMocker = new AutoMocker();
        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateChangeJobTitleNodeRequester(autoMocker, responseMappings);

        var response = await interactor.InvokeAsync(new ChangeJobTitleRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            NewJobTitle = "Senior Software Engineer",
        });

        Assert.IsNotNull(response);

        var mapping = responseMappings["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"];
        Assert.HasCount(1, mapping.Invocations);

        var body = JsonSerializer.Deserialize<JsonElement>(mapping.Invocations[0].Body!);
        Assert.AreEqual("Senior Software Engineer", body.GetProperty("job_title").GetString());
    }

    [TestMethod]
    public async Task Throws_WhenRequestFails()
    {
        var interactor = CreateChangeJobTitleNodeRequester(new AutoMocker(), new() {
            ["(PATCH) http://directories.localhost/users/51a50a75-e4fa-4b6e-9c72-581538ee5258"] =
                new MappedResponse(HttpStatusCode.BadRequest),
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(()
            => interactor.InvokeAsync(new ChangeJobTitleRequest {
                UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
                NewJobTitle = "Senior Software Engineer",
            }));
    }

    [TestMethod]
    public async Task WritesToAudit()
    {
        var autoMocker = new AutoMocker();

        WriteToAuditRequest? capturedWriteToAudit = null;
        autoMocker.CaptureRequest<WriteToAuditRequest>(request => capturedWriteToAudit = request);

        var responseMappings = GetNodeResponseMappingsForHappyPath();
        var interactor = CreateChangeJobTitleNodeRequester(autoMocker, responseMappings);

        await interactor.InvokeAsync(new ChangeJobTitleRequest {
            UserId = new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"),
            NewJobTitle = "Senior Software Engineer",
        });

        Assert.IsNotNull(capturedWriteToAudit);
        Assert.AreEqual(AuditEventCategoryNames.ChangeJobTitle, capturedWriteToAudit.EventCategory);
        Assert.AreEqual("Successfully changed job title to Senior Software Engineer", capturedWriteToAudit.Message);
        Assert.AreEqual(new Guid("51a50a75-e4fa-4b6e-9c72-581538ee5258"), capturedWriteToAudit.UserId);
    }
}
