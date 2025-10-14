using Dfe.SignIn.Core.Contracts.Diagnostics;

namespace Dfe.SignIn.InternalApi.Contracts.UnitTests;

[TestClass]
public sealed class NamingHelpersTests
{
    #region NiceifySchemaId(Type)

    [TestMethod]
    public void NiceifySchemaId_Throws_WhenTypeArgumentIsNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(()
            => NamingHelpers.NiceifySchemaId(null!));
    }

    [TestMethod]
    [DataRow(typeof(InteractionResponse<PingRequest>), "InteractionResponse_PingRequest")]
    [DataRow(typeof(PingRequest), "PingRequest")]
    public void NiceifySchemaId_ReturnsNiceSchemaIdsForGenericTypes(Type genericType, string expectedSchemaId)
    {
        string actualSchemaId = NamingHelpers.NiceifySchemaId(genericType);

        Assert.AreEqual(expectedSchemaId, actualSchemaId);
    }

    #endregion

    #region GetSchemaGroupFromPath(string)

    [TestMethod]
    [DataRow("")]
    [DataRow("SomeEndpoint")]
    public void GetSchemaGroupFromPath_ReturnsDefault_WhenPathIsNotRecognised(string path)
    {
        string group = NamingHelpers.GetSchemaGroupFromPath(path);

        Assert.AreEqual("Default", group);
    }

    [TestMethod]
    [DataRow("interaction/GroupName.UseCaseName", "GroupName")]
    [DataRow("interaction/GroupName.Nested.UseCaseName", "GroupName.Nested")]
    public void GetSchemaGroupFromPath_ReturnsExpectedGroup(string path, string expectedGroup)
    {
        string group = NamingHelpers.GetSchemaGroupFromPath(path);

        Assert.AreEqual(expectedGroup, group);
    }

    #endregion

    #region GetEndpointPath<TRequest>()

    [TestMethod]
    public void GetEndpointPath_Throws_WhenEndpointPathCannotBeResolved()
    {
        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            NamingHelpers.GetEndpointPath<NamingHelpersTests>);

        Assert.AreEqual("Cannot resolve endpoint path for request type.", exception.Message);
    }

    [TestMethod]
    public void GetEndpointPath_ReturnsExpectedEndpointPath()
    {
        string endpointPath = NamingHelpers.GetEndpointPath<PingRequest>();

        Assert.AreEqual("interaction/Diagnostics.Ping", endpointPath);
    }

    #endregion
}
