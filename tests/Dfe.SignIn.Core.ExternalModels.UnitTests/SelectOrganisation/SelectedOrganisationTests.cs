using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;

namespace Dfe.SignIn.Core.ExternalModels.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectedOrganisationTests
{
    #region TryResolveType(string)

    [DataRow(OrganisationDetailLevel.Id, typeof(SelectedOrganisation))]
    [DataRow(OrganisationDetailLevel.Basic, typeof(SelectedOrganisationBasic))]
    [DataRow(OrganisationDetailLevel.Extended, typeof(SelectedOrganisationExtended))]
    [DataRow(OrganisationDetailLevel.Legacy, typeof(SelectedOrganisationLegacy))]
    [DataTestMethod]
    public void TryResolveType_ReturnsExpectedType(OrganisationDetailLevel detailLevel, Type expectedType)
    {
        var result = SelectedOrganisation.TryResolveType(detailLevel);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void TryResolveType_ReturnsNull_WhenPayloadTypeIsUnexpected()
    {
        var result = SelectedOrganisation.TryResolveType(Enum.Parse<OrganisationDetailLevel>("-1"));

        Assert.IsNull(result);
    }

    #endregion

    #region ResolveType(string)

    [DataRow(OrganisationDetailLevel.Id, typeof(SelectedOrganisation))]
    [DataRow(OrganisationDetailLevel.Basic, typeof(SelectedOrganisationBasic))]
    [DataRow(OrganisationDetailLevel.Extended, typeof(SelectedOrganisationExtended))]
    [DataRow(OrganisationDetailLevel.Legacy, typeof(SelectedOrganisationLegacy))]
    [DataTestMethod]
    public void ResolveType_ReturnsExpectedType(OrganisationDetailLevel detailLevel, Type expectedType)
    {
        var result = SelectedOrganisation.ResolveType(detailLevel);

        Assert.AreEqual(expectedType, result);
    }

    [TestMethod]
    public void ResolveType_Throws_WhenPayloadTypeIsUnexpected()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => SelectedOrganisation.ResolveType(Enum.Parse<OrganisationDetailLevel>("-1"))
        );
        Assert.AreEqual("Cannot resolve unknown type '-1'.", exception.Message);
    }

    #endregion
}
