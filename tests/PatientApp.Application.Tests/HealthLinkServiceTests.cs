using FluentAssertions;
using NSubstitute;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;
using PatientApp.Application.Services;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Application.Tests;

public class HealthLinkServiceTests
{
    private readonly IFhirBundleParser _fhirParser;
    private readonly IEncryptionService _encryptionService;
    private readonly IFileStorageService _fileStorage;
    private readonly IJweService _jweService;
    private readonly IHealthLinkSubmissionRepository _repository;
    private readonly HealthLinkService _sut;

    public HealthLinkServiceTests()
    {
        _fhirParser = Substitute.For<IFhirBundleParser>();
        _encryptionService = Substitute.For<IEncryptionService>();
        _fileStorage = Substitute.For<IFileStorageService>();
        _jweService = Substitute.For<IJweService>();
        _repository = Substitute.For<IHealthLinkSubmissionRepository>();

        // Simulate MongoDB generating an Id on CreateAsync
        _repository.When(r => r.CreateAsync(Arg.Any<HealthLinkSubmission>()))
            .Do(ci =>
            {
                var submission = ci.Arg<HealthLinkSubmission>();
                if (string.IsNullOrEmpty(submission.Id))
                    submission.Id = "generated-id";
            });

        _sut = new HealthLinkService(
            _fhirParser,
            _encryptionService,
            _fileStorage,
            _jweService,
            _repository,
            72);
    }

    private static FhirBundleParseResult CreateTestParseResult() => new()
    {
        BundleJson = """{"resourceType": "Bundle"}""",
        PatientName = "Jessica Argonaut",
        PdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }
    };

    private static EncryptionResult CreateTestEncryptionResult() => new()
    {
        Ciphertext = new byte[] { 0x01, 0x02, 0x03 },
        Nonce = new byte[12],
        Tag = new byte[16]
    };

    // --- ProcessBundleAsync ---

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_CallsParserWithBundleJson()
    {
        // Arrange
        var bundleJson = """{"resourceType": "Bundle"}""";
        _fhirParser.Parse(bundleJson).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        await _sut.ProcessBundleAsync(bundleJson, "https://example.com");

        // Assert
        _fhirParser.Received(1).Parse(bundleJson);
    }

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_EncryptsBundleAndPdf()
    {
        // Arrange
        _fhirParser.Parse(Arg.Any<string>()).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        await _sut.ProcessBundleAsync("""{"resourceType": "Bundle"}""", "https://example.com");

        // Assert
        _encryptionService.Received(2).Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>());
    }

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_WritesTwoEncryptedFiles()
    {
        // Arrange
        _fhirParser.Parse(Arg.Any<string>()).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        await _sut.ProcessBundleAsync("""{"resourceType": "Bundle"}""", "https://example.com");

        // Assert
        await _fileStorage.Received(2).WriteFileAsync(Arg.Any<string>(), Arg.Any<byte[]>());
        await _fileStorage.Received(1).WriteFileAsync(
            Arg.Is<string>(s => s.EndsWith("/bundle.enc")), Arg.Any<byte[]>());
        await _fileStorage.Received(1).WriteFileAsync(
            Arg.Is<string>(s => s.EndsWith("/document.enc")), Arg.Any<byte[]>());
    }

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_PersistsSubmission()
    {
        // Arrange
        _fhirParser.Parse(Arg.Any<string>()).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        await _sut.ProcessBundleAsync("""{"resourceType": "Bundle"}""", "https://example.com");

        // Assert
        await _repository.Received(1).CreateAsync(Arg.Is<HealthLinkSubmission>(s =>
            s.PatientName == "Jessica Argonaut" &&
            s.EncryptionKey.Length == 32));
    }

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_UpdatesFilePathsAfterCreate()
    {
        // Arrange
        _fhirParser.Parse(Arg.Any<string>()).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        await _sut.ProcessBundleAsync("""{"resourceType": "Bundle"}""", "https://example.com");

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<HealthLinkSubmission>(s =>
            s.BundleFilePath == "generated-id/bundle.enc" &&
            s.PdfFilePath == "generated-id/document.enc"));
    }

    [Fact]
    public async Task Given_ValidBundle_When_ProcessBundleAsync_Then_ReturnsShlWithCorrectStructure()
    {
        // Arrange
        _fhirParser.Parse(Arg.Any<string>()).Returns(CreateTestParseResult());
        _encryptionService.Encrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(CreateTestEncryptionResult());

        // Act
        var result = await _sut.ProcessBundleAsync("""{"resourceType": "Bundle"}""", "https://example.com");

        // Assert
        result.Flag.Should().Be("U");
        result.Key.Should().NotBeNullOrEmpty();
        result.Key.Should().HaveLength(43); // 32 bytes base64url-encoded = 43 chars
        result.Url.Should().Be("https://example.com/api/v1/healthlinks/generated-id");
        result.Label.Should().Contain("Jessica Argonaut");
        result.Exp.Should().BeGreaterThan(0);
    }

    // --- RetrieveAsync ---

    [Fact]
    public async Task Given_SubmissionDoesNotExist_When_RetrieveAsync_Then_ReturnsNotFound()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((HealthLinkSubmission?)null);

        // Act
        var result = await _sut.RetrieveAsync("nonexistent", "Test Hospital");

        // Assert
        result.Found.Should().BeFalse();
    }

    [Fact]
    public async Task Given_ExpiredSubmission_When_RetrieveAsync_Then_ReturnsExpired()
    {
        // Arrange
        var submission = new HealthLinkSubmission
        {
            Id = "test-id",
            PatientName = "Jessica Argonaut",
            EncryptionKey = new byte[32],
            BundleNonce = new byte[12],
            BundleTag = new byte[16],
            BundleFilePath = "test-id/bundle.enc",
            PdfNonce = new byte[12],
            PdfTag = new byte[16],
            PdfFilePath = "test-id/document.enc",
            ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        _repository.GetByIdAsync("test-id").Returns(submission);

        // Act
        var result = await _sut.RetrieveAsync("test-id", "Test Hospital");

        // Assert
        result.Found.Should().BeTrue();
        result.Expired.Should().BeTrue();
    }

    [Fact]
    public async Task Given_ValidSubmission_When_RetrieveAsync_Then_ReturnsJwe()
    {
        // Arrange
        var key = new byte[32];
        var nonce = new byte[12];
        var tag = new byte[16];
        var submission = new HealthLinkSubmission
        {
            Id = "test-id",
            PatientName = "Jessica Argonaut",
            EncryptionKey = key,
            BundleNonce = nonce,
            BundleTag = tag,
            BundleFilePath = "test-id/bundle.enc",
            PdfNonce = new byte[12],
            PdfTag = new byte[16],
            PdfFilePath = "test-id/document.enc",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _repository.GetByIdAsync("test-id").Returns(submission);

        var encryptedBundle = new byte[] { 0x01, 0x02, 0x03 };
        _fileStorage.ReadFileAsync("test-id/bundle.enc").Returns(encryptedBundle);

        var decryptedBundle = new byte[] { 0x7B, 0x7D }; // "{}"
        _encryptionService.Decrypt(encryptedBundle, key, nonce, tag).Returns(decryptedBundle);

        _jweService.BuildJweCompactSerialization(decryptedBundle, key, "application/fhir+json")
            .Returns("eyJ...compact-jwe");

        // Act
        var result = await _sut.RetrieveAsync("test-id", "Test Hospital");

        // Assert
        result.Found.Should().BeTrue();
        result.Expired.Should().BeFalse();
        result.JweCompactSerialization.Should().Be("eyJ...compact-jwe");
    }

    [Fact]
    public async Task Given_ValidSubmission_When_RetrieveAsync_Then_ReadsEncryptedFileFromStorage()
    {
        // Arrange
        var submission = new HealthLinkSubmission
        {
            Id = "test-id",
            PatientName = "Jessica Argonaut",
            EncryptionKey = new byte[32],
            BundleNonce = new byte[12],
            BundleTag = new byte[16],
            BundleFilePath = "test-id/bundle.enc",
            PdfNonce = new byte[12],
            PdfTag = new byte[16],
            PdfFilePath = "test-id/document.enc",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _repository.GetByIdAsync("test-id").Returns(submission);
        _fileStorage.ReadFileAsync("test-id/bundle.enc").Returns(new byte[] { 0x01 });
        _encryptionService.Decrypt(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<byte[]>())
            .Returns(new byte[] { 0x7B, 0x7D });
        _jweService.BuildJweCompactSerialization(Arg.Any<byte[]>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns("jwe");

        // Act
        await _sut.RetrieveAsync("test-id", "Test Hospital");

        // Assert
        await _fileStorage.Received(1).ReadFileAsync("test-id/bundle.enc");
    }
}
