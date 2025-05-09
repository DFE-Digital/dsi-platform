using Dfe.SignIn.Core.ExternalModels.Organisations;

namespace Dfe.SignIn.Core.ExternalModels.UnitTests.Organisations;

[TestClass]
public sealed class OrganisationDetailsTests
{
    #region TryResolveType(string)

    [DataRow(OrganisationDetailLevel.Id, typeof(OrganisationDetails))]
    [DataRow(OrganisationDetailLevel.Basic, typeof(OrganisationDetailsBasic))]
    [DataRow(OrganisationDetailLevel.Extended, typeof(OrganisationDetailsExtended))]
    [DataRow(OrganisationDetailLevel.Legacy, typeof(OrganisationDetailsLegacy))]
    [DataTestMethod]
    public void TryResolveType_ReturnsExpectedType(OrganisationDetailLevel detailLevel, Type expectedType)
    {
        var result = OrganisationDetails.TryResolveType(detailLevel);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void TryResolveType_ReturnsNull_WhenPayloadTypeIsUnexpected()
    {
        var result = OrganisationDetails.TryResolveType(Enum.Parse<OrganisationDetailLevel>("-1"));

        Assert.IsNull(result);
    }

    #endregion

    #region ResolveType(string)

    [DataRow(OrganisationDetailLevel.Id, typeof(OrganisationDetails))]
    [DataRow(OrganisationDetailLevel.Basic, typeof(OrganisationDetailsBasic))]
    [DataRow(OrganisationDetailLevel.Extended, typeof(OrganisationDetailsExtended))]
    [DataRow(OrganisationDetailLevel.Legacy, typeof(OrganisationDetailsLegacy))]
    [DataTestMethod]
    public void ResolveType_ReturnsExpectedType(OrganisationDetailLevel detailLevel, Type expectedType)
    {
        var result = OrganisationDetails.ResolveType(detailLevel);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void ResolveType_Throws_WhenPayloadTypeIsUnexpected()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => OrganisationDetails.ResolveType(Enum.Parse<OrganisationDetailLevel>("-1"))
        );
        Assert.AreEqual("Cannot resolve unknown type '-1'.", exception.Message);
    }

    #endregion
}
