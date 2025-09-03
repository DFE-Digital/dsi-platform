using System.Net;
using System.Text;
using System.Text.Json;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.SupportTickets;
using Dfe.SignIn.NodeApi.Client.UnitTests.Fakes;

namespace Dfe.SignIn.NodeApi.Client.UnitTests.SupportTickets;

[TestClass]
public sealed class GetApplicationNamesForSupportTicketNodeRequesterTests
{
    private static readonly RelyingPartyDto FakeRelyingPartyDetails = new() {
        ClientId = "xyz",
        ClientSecret = "xyz",
        ServiceHome = "https://example.com",
    };

    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetApplicationNamesForSupportTicketRequest,
            GetApplicationNamesForSupportTicketNodeRequester
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedApplicationNames()
    {
        var mockDto = new ApplicationListingDto() {
            Services = [
                new() {
                    Id = new Guid("63987e25-89ae-4908-b342-5b59fd76c44f"),
                    Name = "Example Service A",
                    RelyingParty = FakeRelyingPartyDetails,
                },
                new() {
                    Id = new Guid("4a38ba83-bcc5-4a50-be20-681dca501d89"),
                    Name = "Example Service B",
                    RelyingParty = FakeRelyingPartyDetails with {
                        Params = new() {
                            HelpHidden = "true",
                        },
                    },
                },
                new() {
                    Id = new Guid("1d152762-579f-4bf3-91f9-288222f1b2bb"),
                    Name = "Example Service C",
                    RelyingParty = FakeRelyingPartyDetails with {
                        Params = new() {
                            HelpHidden = "false",
                        },
                    },
                },
            ],
        };

        Uri? capturedRequestUri = null;
        var testHandler = new FakeHttpMessageHandler((req, ct) => {
            capturedRequestUri = req.RequestUri;
            var response = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(JsonSerializer.Serialize(mockDto), Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(testHandler) {
            BaseAddress = new Uri("http://mock.localhost")
        };

        var controller = new GetApplicationNamesForSupportTicketNodeRequester(client);

        var response = await controller.InvokeAsync(new GetApplicationNamesForSupportTicketRequest());

        Assert.IsNotNull(capturedRequestUri);
        Assert.AreEqual("/services?page=1&pageSize=1000", capturedRequestUri.PathAndQuery);

        Assert.AreEqual(response.Applications.ElementAt(0), new ApplicationNameForSupportTicket {
            Name = "Example Service A",
        });
        Assert.AreEqual(response.Applications.ElementAt(1), new ApplicationNameForSupportTicket {
            Name = "Example Service C",
        });
        Assert.AreEqual(response.Applications.ElementAt(2), new ApplicationNameForSupportTicket {
            Name = "Other (please specify)",
        });
        Assert.AreEqual(response.Applications.ElementAt(3), new ApplicationNameForSupportTicket {
            Name = "None",
        });
    }
}
