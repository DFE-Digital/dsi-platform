using Dfe.SignIn.WebFramework.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Dfe.SignIn.WebFramework.UnitTests.Configuration;

[TestClass]
public sealed class SecurityHeaderPolicyExtensionsTests
{
    private TestServer server = null!;
    private HttpClient client = null!;

    [TestInitialize]
    public void Initialize()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services => { })
            .Configure(app => {
                app.UseDsiSecurityHeaderPolicy();
                app.Run(context => {
                    context.Response.Headers.Append("Server", "Test");
                    return context.Response.WriteAsync("Hello World!");
                });
            });

        this.server = new TestServer(builder);
        this.client = this.server.CreateClient();
        this.client.BaseAddress = new Uri("https://test");
    }

    [TestCleanup]
    public void Cleanup()
    {
        this.client?.Dispose();
        this.server?.Dispose();
    }

    [TestMethod]
    public void UseDsiSecurityHeaderPolicy_Throws_WhenBuilderArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => SecurityHeaderPolicyExtensions.UseDsiSecurityHeaderPolicy(null!));
    }

    private async Task<HttpResponseMessage> MakeTestRequest()
    {
        var response = await this.client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        return response;
    }

    private async Task AssertHeader(string headerName, string expectedValue)
    {
        var response = await this.MakeTestRequest();

        Assert.IsTrue(response.Headers.Contains(headerName));
        Assert.AreEqual(expectedValue, response.Headers.GetValues(headerName).First());
    }

    private async Task AssertNoHeader(string headerName)
    {
        var response = await this.MakeTestRequest();

        Assert.IsFalse(response.Headers.Contains(headerName));
    }

    private async Task AssertContentSecurityPolicy(string key, string[] expectedValues, string[]? expectedPartialMatches = null)
    {
        var response = await this.MakeTestRequest();

        string header = response.Headers.GetValues("Content-Security-Policy").First();
        var policy = header
            .Split(";")
            .Select(src => {
                var srcParts = src.Trim().Split(" ").Select(part => part.Trim());
                return new {
                    Key = srcParts.First(),
                    Parts = srcParts.Skip(1),
                };
            })
            .ToDictionary(e => e.Key, e => e.Parts);

        foreach (string expectedValue in expectedValues) {
            string? actualValue = policy[key].FirstOrDefault(part => part == expectedValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        foreach (string expectedPartialValue in expectedPartialMatches ?? []) {
            string? actualValue = policy[key].FirstOrDefault(part => part.Contains(expectedPartialValue));
            Assert.IsNotNull(actualValue);
        }
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_PreventsInternetExplorerUsersExecutingDownloadsInSiteContext()
    {
        return this.AssertHeader("X-Download-Options", "noopen");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_SetsHstsMaximumAge()
    {
        return this.AssertHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_DisablesXssProtection()
    {
        return this.AssertHeader("X-XSS-Protection", "0");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_DoNotAttemptToGuessContentType()
    {
        return this.AssertHeader("X-Content-Type-Options", "nosniff");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_NoReferrerInformation()
    {
        return this.AssertHeader("Referrer-Policy", "no-referrer");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_RemoveServerHeader()
    {
        return this.AssertNoHeader("Server");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_RestrictOpenerPolicyToSameOrigin()
    {
        return this.AssertHeader("Cross-Origin-Opener-Policy", "same-origin");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_RequireCrossOriginResourcePolicyForEmbeddedResources()
    {
        return this.AssertHeader("Cross-Origin-Embedder-Policy", "require-corp");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_RequireCrossOriginResourcePolicyForResources()
    {
        return this.AssertHeader("Cross-Origin-Resource-Policy", "same-origin");
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_SpecifiesDefaultSrcPolicy()
    {
        return this.AssertContentSecurityPolicy("default-src", ["'self'", "*.signin.education.gov.uk"]);
    }

    [TestMethod]
    public Task UseDsiSecurityHeaderPolicy_SpecifiesScriptSrcPolicy()
    {
        return this.AssertContentSecurityPolicy("script-src", ["'self'", "*.signin.education.gov.uk"], ["nonce-"]);
    }
}
