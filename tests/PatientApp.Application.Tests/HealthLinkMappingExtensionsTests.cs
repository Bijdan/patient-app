using FluentAssertions;
using PatientApp.Application.Mappings;
using PatientApp.Domain.Entities;

namespace PatientApp.Application.Tests;

public class HealthLinkMappingExtensionsTests
{
    [Fact]
    public void Given_Submission_When_MappedToShlDto_Then_AllFieldsAreCopied()
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
            ExpiresAt = new DateTime(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var dto = submission.ToShlDto("https://example.com/api/v1/healthlinks/test-id", "test-key-base64url");

        // Assert
        dto.Url.Should().Be("https://example.com/api/v1/healthlinks/test-id");
        dto.Flag.Should().Be("U");
        dto.Key.Should().Be("test-key-base64url");
        dto.Exp.Should().Be(new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds());
    }

    [Fact]
    public void Given_Submission_When_MappedToShlDto_Then_LabelContainsPatientName()
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
            ExpiresAt = DateTime.UtcNow.AddHours(72)
        };

        // Act
        var dto = submission.ToShlDto("https://example.com/api/v1/healthlinks/test-id", "key");

        // Assert
        dto.Label.Should().Be("Jessica Argonaut's health summary");
    }

    [Fact]
    public void Given_Submission_When_MappedToShlDto_Then_FlagIsAlwaysU()
    {
        // Arrange
        var submission = new HealthLinkSubmission
        {
            Id = "test-id",
            PatientName = "Test Patient",
            EncryptionKey = new byte[32],
            BundleNonce = new byte[12],
            BundleTag = new byte[16],
            BundleFilePath = "test-id/bundle.enc",
            PdfNonce = new byte[12],
            PdfTag = new byte[16],
            PdfFilePath = "test-id/document.enc",
            ExpiresAt = DateTime.UtcNow.AddHours(72)
        };

        // Act
        var dto = submission.ToShlDto("url", "key");

        // Assert
        dto.Flag.Should().Be("U");
    }
}
