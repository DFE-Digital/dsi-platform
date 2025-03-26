using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackErrorExceptionTests
{
    #region Property: ErrorCode

    [TestMethod]
    public void ErrorCode_HasInitializedValue()
    {
        var exception = new SelectOrganisationCallbackErrorException(SelectOrganisationErrorCode.InvalidSelection);

        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, exception.ErrorCode);
    }

    #endregion
}
