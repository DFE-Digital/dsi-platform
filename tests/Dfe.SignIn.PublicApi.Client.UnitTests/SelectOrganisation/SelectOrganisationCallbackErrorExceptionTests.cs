using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.PublicApi.Client.SelectOrganisation;

namespace Dfe.SignIn.PublicApi.Client.UnitTests.SelectOrganisation;

[TestClass]
public sealed class SelectOrganisationCallbackErrorExceptionTests
{
    #region Property: ErrorCode

    [TestMethod]
    public void ErrorCode_HasInitializedValue()
    {
        var exception = new SelectOrganisationCallbackErrorException(null, SelectOrganisationErrorCode.InvalidSelection);

        Assert.AreEqual(SelectOrganisationErrorCode.InvalidSelection, exception.ErrorCode);
    }

    #endregion
}
