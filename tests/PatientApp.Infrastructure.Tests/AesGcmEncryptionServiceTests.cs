using System.Security.Cryptography;
using FluentAssertions;
using PatientApp.Infrastructure.Services;

namespace PatientApp.Infrastructure.Tests;

public class AesGcmEncryptionServiceTests
{
    private readonly AesGcmEncryptionService _sut;

    public AesGcmEncryptionServiceTests()
    {
        _sut = new AesGcmEncryptionService();
    }

    [Fact]
    public void Given_Plaintext_When_EncryptThenDecrypt_Then_ReturnsOriginalPlaintext()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, FHIR World!");

        // Act
        var encrypted = _sut.Encrypt(plaintext, key);
        var decrypted = _sut.Decrypt(encrypted.Ciphertext, key, encrypted.Nonce, encrypted.Tag);

        // Assert
        decrypted.Should().BeEquivalentTo(plaintext);
    }

    [Fact]
    public void Given_Plaintext_When_Encrypt_Then_NonceIs12Bytes()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var result = _sut.Encrypt(plaintext, key);

        // Assert
        result.Nonce.Should().HaveCount(12);
    }

    [Fact]
    public void Given_Plaintext_When_Encrypt_Then_TagIs16Bytes()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var result = _sut.Encrypt(plaintext, key);

        // Assert
        result.Tag.Should().HaveCount(16);
    }

    [Fact]
    public void Given_Plaintext_When_Encrypt_Then_CiphertextHasSameLengthAsPlaintext()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

        // Act
        var result = _sut.Encrypt(plaintext, key);

        // Assert
        result.Ciphertext.Should().HaveCount(plaintext.Length);
    }

    [Fact]
    public void Given_EncryptedData_When_DecryptWithWrongKey_Then_ThrowsCryptographicException()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var wrongKey = RandomNumberGenerator.GetBytes(32);
        var plaintext = System.Text.Encoding.UTF8.GetBytes("Sensitive data");

        var encrypted = _sut.Encrypt(plaintext, key);

        // Act
        var act = () => _sut.Decrypt(encrypted.Ciphertext, wrongKey, encrypted.Nonce, encrypted.Tag);

        // Assert
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Given_LargePayload_When_EncryptThenDecrypt_Then_ReturnsOriginalData()
    {
        // Arrange
        var key = RandomNumberGenerator.GetBytes(32);
        var plaintext = RandomNumberGenerator.GetBytes(100_000);

        // Act
        var encrypted = _sut.Encrypt(plaintext, key);
        var decrypted = _sut.Decrypt(encrypted.Ciphertext, key, encrypted.Nonce, encrypted.Tag);

        // Assert
        decrypted.Should().BeEquivalentTo(plaintext);
    }
}
