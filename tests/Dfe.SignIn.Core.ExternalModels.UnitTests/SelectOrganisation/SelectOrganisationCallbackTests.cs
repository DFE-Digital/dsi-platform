using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Core.ExternalModels.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackViewModelTests
{
    #region TryResolveType(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TryResolveType_Throws_WhenPayloadTypeArgumentIsNull()
    {
        SelectOrganisationCallback.TryResolveType(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TryResolveType_Throws_WhenPayloadTypeArgumentIsEmptyString()
    {
        SelectOrganisationCallback.TryResolveType("   ");
    }

    [DataRow(PayloadTypeConstants.Error, typeof(SelectOrganisationCallbackError))]
    [DataRow(PayloadTypeConstants.SignOut, typeof(SelectOrganisationCallbackSignOut))]
    [DataRow(PayloadTypeConstants.Cancel, typeof(SelectOrganisationCallbackCancel))]
    [DataRow(PayloadTypeConstants.Id, typeof(SelectOrganisationCallbackId))]
    [DataRow(PayloadTypeConstants.Basic, typeof(SelectOrganisationCallbackBasic))]
    [DataRow(PayloadTypeConstants.Extended, typeof(SelectOrganisationCallbackExtended))]
    [DataRow(PayloadTypeConstants.Legacy, typeof(SelectOrganisationCallbackLegacy))]
    [DataTestMethod]
    public void TryResolveType_ReturnsExpectedType(string payloadType, Type expectedType)
    {
        var result = SelectOrganisationCallback.TryResolveType(payloadType);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void TryResolveType_ReturnsNull_WhenPayloadTypeIsUnexpected()
    {
        var result = SelectOrganisationCallback.TryResolveType("unexpectedType");

        Assert.IsNull(result);
    }

    #endregion

    #region ResolveType(string)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void ResolveType_Throws_WhenPayloadTypeArgumentIsNull()
    {
        SelectOrganisationCallback.ResolveType(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ResolveType_Throws_WhenPayloadTypeArgumentIsEmptyString()
    {
        SelectOrganisationCallback.ResolveType("   ");
    }

    [DataRow(PayloadTypeConstants.Error, typeof(SelectOrganisationCallbackError))]
    [DataRow(PayloadTypeConstants.SignOut, typeof(SelectOrganisationCallbackSignOut))]
    [DataRow(PayloadTypeConstants.Cancel, typeof(SelectOrganisationCallbackCancel))]
    [DataRow(PayloadTypeConstants.Id, typeof(SelectOrganisationCallbackId))]
    [DataRow(PayloadTypeConstants.Basic, typeof(SelectOrganisationCallbackBasic))]
    [DataRow(PayloadTypeConstants.Extended, typeof(SelectOrganisationCallbackExtended))]
    [DataRow(PayloadTypeConstants.Legacy, typeof(SelectOrganisationCallbackLegacy))]
    [DataTestMethod]
    public void ResolveType_ReturnsExpectedType(string payloadType, Type expectedType)
    {
        var result = SelectOrganisationCallback.TryResolveType(payloadType);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void ResolveType_Throws_WhenPayloadTypeIsUnexpected()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => SelectOrganisationCallback.ResolveType("unexpectedType")
        );
        Assert.AreEqual("Cannot resolve unknown payload type 'unexpectedType'.", exception.Message);
    }

    #endregion
}
