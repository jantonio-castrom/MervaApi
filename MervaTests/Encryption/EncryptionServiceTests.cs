using MervaApi.Encryption.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MervaTests.Encryption;

public class EncryptionServiceTests
{
    private static EncryptionService Create(byte[]? key = null)
    {
        var keyBase64 = Convert.ToBase64String(key ?? new byte[32]);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "USERTOKEN_KEY", keyBase64 } })
            .Build();
        return new EncryptionService(config);
    }

    [Fact]
    public void EncryptDecrypt_Roundtrip_ReturnsOriginalPlaintext()
    {
        var svc = Create();
        var plaintext = "hello world";
        Assert.Equal(plaintext, svc.Decrypt(svc.Encrypt(plaintext)));
    }

    [Fact]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphertexts()
    {
        var svc = Create();
        Assert.NotEqual(svc.Encrypt("test"), svc.Encrypt("test"));
    }

    [Fact]
    public void ComputeSha256_SameInput_AlwaysProducesSameHash()
    {
        var svc = Create();
        Assert.Equal(svc.ComputeSha256("token"), svc.ComputeSha256("token"));
    }

    [Fact]
    public void ComputeSha256_DifferentInputs_ProduceDifferentHashes()
    {
        var svc = Create();
        Assert.NotEqual(svc.ComputeSha256("a"), svc.ComputeSha256("b"));
    }

    [Fact]
    public void ComputeSha256_Returns32Bytes()
    {
        var svc = Create();
        Assert.Equal(32, svc.ComputeSha256("anything").Length);
    }

    [Fact]
    public void Constructor_MissingKey_Throws()
    {
        var config = new ConfigurationBuilder().Build();
        Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
    }

    [Fact]
    public void Constructor_KeyNot32Bytes_Throws()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                { "USERTOKEN_KEY", Convert.ToBase64String(new byte[16]) }
            })
            .Build();
        Assert.Throws<InvalidOperationException>(() => new EncryptionService(config));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("unicode: 🔐 café naïve")]
    [InlineData("a very long string that keeps going and going and going to test block boundaries in AES")]
    public void EncryptDecrypt_VariousInputs_RoundtripCorrectly(string plaintext)
    {
        var svc = Create();
        Assert.Equal(plaintext, svc.Decrypt(svc.Encrypt(plaintext)));
    }
}
