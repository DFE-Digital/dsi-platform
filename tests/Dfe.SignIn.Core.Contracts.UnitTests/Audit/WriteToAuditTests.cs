using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;

namespace Dfe.SignIn.Core.Contracts.UnitTests.Audit;

[TestClass]
public sealed class WriteToAuditTests
{
    [TestMethod]
    public void IsAnnotatedAsNonCancellable()
    {
        Assert.IsTrue(
            Attribute.IsDefined(typeof(WriteToAuditRequest), typeof(NonCancellableAttribute))
        );
    }
}
