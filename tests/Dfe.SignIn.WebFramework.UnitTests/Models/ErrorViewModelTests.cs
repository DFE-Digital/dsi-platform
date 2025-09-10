using Dfe.SignIn.WebFramework.Models;

namespace Dfe.SignIn.WebFramework.UnitTests.Models;

[TestClass]
public sealed class ErrorViewModelTests
{
    #region ShowRequestId { get; }

    [TestMethod]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsProvided()
    {
        var viewModel = new ErrorViewModel {
            RequestId = "26feedcd-a584-43df-99d0-7d865047aa80",
        };

        Assert.IsTrue(viewModel.ShowRequestId);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNotProvided(string? requestId)
    {
        var viewModel = new ErrorViewModel {
            RequestId = requestId,
        };

        Assert.IsFalse(viewModel.ShowRequestId);
    }

    #endregion
}
