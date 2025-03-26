using System.Security.Cryptography;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;
using Dfe.SignIn.PublicApi.Client.PublicApiSigning;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.PublicApi.Client.UnitTests;

[TestClass]
public sealed class DefaultPayloadVerifierTests
{
    #region VerifyPayload(string, PayloadDigitalSignature)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task VerifyPayload_Throws_WhenDataArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var payloadVerifier = autoMocker.CreateInstance<DefaultPayloadVerifier>();

        await payloadVerifier.VerifyPayload(null!, new PayloadDigitalSignature {
            KeyId = "<key_id>",
            Signature = "<signature>",
        });
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task VerifyPayload_Throws_WhenSignatureArgumentIsNull()
    {
        var autoMocker = new AutoMocker();

        var payloadVerifier = autoMocker.CreateInstance<DefaultPayloadVerifier>();

        await payloadVerifier.VerifyPayload("<data>", null!);
    }

    [TestMethod]
    public async Task VerifyPayload_ReturnsFalse_WhenPublicKeyWasNotFound()
    {
        var autoMocker = new AutoMocker();

        autoMocker.GetMock<IPublicKeyCache>()
            .Setup(x => x.GetPublicKeyAsync(
                It.Is<string>(keyId => keyId == "FakePublicKey1")
            ))
            .ReturnsAsync((PublicKeyCacheEntry?)null);

        var payloadVerifier = autoMocker.CreateInstance<DefaultPayloadVerifier>();

        bool result = await payloadVerifier.VerifyPayload("<data>", new PayloadDigitalSignature {
            KeyId = "<key_id>",
            Signature = "<signature>",
        });

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task VerifyPayload_Throws_WhenUnexpectedHashAlgorithmIsEncountered()
    {
        var autoMocker = new AutoMocker();

        var fakePublicKey = new WellKnownPublicKey {
            Kid = "FakePublicKey1",
            Kty = "RSA",
            Alg = "SomeUnexpectedAlgorithm",
            Use = "sig",
            N = "<n>",
            E = "<e>",
            Ed = 0,
        };

        autoMocker.GetMock<IPublicKeyCache>()
            .Setup(x => x.GetPublicKeyAsync(
                It.Is<string>(keyId => keyId == "FakePublicKey1")
            ))
            .ReturnsAsync(new PublicKeyCacheEntry(fakePublicKey, null!));

        var payloadVerifier = autoMocker.CreateInstance<DefaultPayloadVerifier>();

        var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => payloadVerifier.VerifyPayload("<data>", new PayloadDigitalSignature {
                KeyId = "FakePublicKey1",
                Signature = "<signature>",
            })
        );
        Assert.AreEqual("Unexpected hash algorithm 'SomeUnexpectedAlgorithm'.", exception.Message);
    }

    [TestMethod]
    public async Task VerifyPayload_ReturnsTrue_WhenSignatureWasVerified()
    {
        var autoMocker = new AutoMocker();

        var fakePublicKey = new WellKnownPublicKey {
            Kid = "FakePublicKey1",
            Kty = "RSA",
            Alg = "RS256",
            Use = "sig",
            N = "wg3F2vOvN5clme1SpJ6RCr_BhcRyIrubkT7zjZh-tOcr1BFZRDUkpIgan3POdMBsuzRqHlA6f1pXeS" +
                "eS-Ba5wEiyU6K3nkXeQAc-hle-Vz-QVDLFjiIbWAz6qeKEPPRr8kKAOEDUebKPbIcroqxYPV6EkJ_n" +
                "iOU5_yruw9cn3bruj1JFtiFa1eHMMInmGtXBvmcgiTsw-3dp5SHwZahpGIF7z7XnqCTemgbHhm08EV" +
                "GPIPp_mptzG7i0qGdd934SuCaJoVhoNPM5UqtXfMAwH0Rnq1qTvZX-UX1x688bJiMgndhUXddISNBa" +
                "GYxl8-TZY_LUAa_whicY4cg1RJopZw",
            E = "AQAB",
            Ed = 0,
        };

        using var rsa = RSA.Create(new RSAParameters {
            Modulus = Base64UrlEncoder.DecodeBytes(fakePublicKey.N),
            Exponent = Base64UrlEncoder.DecodeBytes(fakePublicKey.E),
        });

        autoMocker.GetMock<IPublicKeyCache>()
            .Setup(x => x.GetPublicKeyAsync(
                It.Is<string>(keyId => keyId == "FakePublicKey1")
            ))
            .ReturnsAsync(new PublicKeyCacheEntry(fakePublicKey, rsa));

        var payloadVerifier = autoMocker.CreateInstance<DefaultPayloadVerifier>();

        bool result = await payloadVerifier.VerifyPayload("Hello, world!", new PayloadDigitalSignature {
            KeyId = "FakePublicKey1",
            Signature = "" +
                "ZZr5T7BhIhmVabON1T+ASYvtj7apkyWaCGtS/Sw0KtuX1AKv1SvcxuM8Gy6EIjjbyts9tsv2O5fgUR" +
                "bmOfj9sY8mZxLxsHXGXNUiuh+v4o2S9Lpgp4gJbw7vgPs/iTXENZ09EgBl8479hpW4dDuZzU8m/iKH" +
                "gvL3KUCjD9PXp1AZGb0dgm2V3/4l2gJoMPhbB4IGJSHfv8gRja7GMy3wl6lbOtrI485P7pzFC7K/G2" +
                "hgGzhngvyJuWgO8+CHM9tkTGyp996Skl/Q8fKz6z4W8JJAGIcGKP0FhdEPDJPvMB5tN5F/0Rkb/JHK" +
                "rDD5+/eJf5JAY6ICB4v4DpA2amio1w==",
        });

        Assert.IsTrue(result);
    }

    #endregion
}
