using FluentAssertions;
using PatientApp.Application.DTOs;
using PatientApp.Application.Mappings;
using PatientApp.Domain.Entities;

namespace PatientApp.Application.Tests;

public class PatientMappingExtensionsTests
{
    // --- ToDto ---

    [Fact]
    public void Given_Patient_When_MappedToDto_Then_AllFieldsAreCopied()
    {
        // Arrange
        var patient = new Patient
        {
            Id = "507f1f77bcf86cd799439011",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com",
            Phone = "+1-555-0101",
            CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var dto = patient.ToDto();

        // Assert
        dto.Id.Should().Be(patient.Id);
        dto.FirstName.Should().Be(patient.FirstName);
        dto.LastName.Should().Be(patient.LastName);
        dto.DateOfBirth.Should().Be(patient.DateOfBirth);
        dto.Email.Should().Be(patient.Email);
        dto.Phone.Should().Be(patient.Phone);
        dto.CreatedAt.Should().Be(patient.CreatedAt);
        dto.UpdatedAt.Should().Be(patient.UpdatedAt);
    }

    [Fact]
    public void Given_PatientWithNullPhone_When_MappedToDto_Then_PhoneIsNull()
    {
        // Arrange
        var patient = new Patient
        {
            Id = "507f1f77bcf86cd799439011",
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 7, 22),
            Email = "jane.smith@example.com",
            Phone = null
        };

        // Act
        var dto = patient.ToDto();

        // Assert
        dto.Phone.Should().BeNull();
    }

    // --- ToEntity ---

    [Fact]
    public void Given_CreateRequest_When_MappedToEntity_Then_FieldsAreCopied()
    {
        // Arrange
        var request = new CreatePatientRequest
        {
            FirstName = "Robert",
            LastName = "Johnson",
            DateOfBirth = new DateTime(1978, 11, 8),
            Email = "robert.johnson@example.com",
            Phone = "+1-555-0103"
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        entity.FirstName.Should().Be(request.FirstName);
        entity.LastName.Should().Be(request.LastName);
        entity.DateOfBirth.Should().Be(request.DateOfBirth);
        entity.Email.Should().Be(request.Email);
        entity.Phone.Should().Be(request.Phone);
    }

    [Fact]
    public void Given_CreateRequest_When_MappedToEntity_Then_TimestampsAreSetToUtcNow()
    {
        // Arrange
        var request = new CreatePatientRequest
        {
            FirstName = "Emily",
            LastName = "Williams",
            DateOfBirth = new DateTime(1995, 1, 30),
            Email = "emily.williams@example.com"
        };
        var before = DateTime.UtcNow;

        // Act
        var entity = request.ToEntity();

        // Assert
        var after = DateTime.UtcNow;
        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        entity.UpdatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Given_CreateRequest_When_MappedToEntity_Then_CreatedAtEqualsUpdatedAt()
    {
        // Arrange
        var request = new CreatePatientRequest
        {
            FirstName = "Michael",
            LastName = "Brown",
            DateOfBirth = new DateTime(1982, 9, 12),
            Email = "michael.brown@example.com"
        };

        // Act
        var entity = request.ToEntity();

        // Assert
        entity.CreatedAt.Should().Be(entity.UpdatedAt);
    }

    // --- UpdateFrom ---

    [Fact]
    public void Given_ExistingPatient_When_UpdatedFromRequest_Then_FieldsAreOverwritten()
    {
        // Arrange
        var patient = new Patient
        {
            Id = "507f1f77bcf86cd799439011",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com",
            Phone = "+1-555-0101",
            CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var request = new UpdatePatientRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe-Smith",
            DateOfBirth = new DateTime(1985, 3, 16),
            Email = "jonathan.doe@example.com",
            Phone = "+1-555-9999"
        };

        // Act
        patient.UpdateFrom(request);

        // Assert
        patient.FirstName.Should().Be("Jonathan");
        patient.LastName.Should().Be("Doe-Smith");
        patient.DateOfBirth.Should().Be(new DateTime(1985, 3, 16));
        patient.Email.Should().Be("jonathan.doe@example.com");
        patient.Phone.Should().Be("+1-555-9999");
    }

    [Fact]
    public void Given_ExistingPatient_When_UpdatedFromRequest_Then_UpdatedAtIsRefreshed()
    {
        // Arrange
        var originalUpdatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var patient = new Patient
        {
            Id = "507f1f77bcf86cd799439011",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com",
            UpdatedAt = originalUpdatedAt
        };

        var request = new UpdatePatientRequest
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com"
        };

        var before = DateTime.UtcNow;

        // Act
        patient.UpdateFrom(request);

        // Assert
        patient.UpdatedAt.Should().BeOnOrAfter(before);
        patient.UpdatedAt.Should().NotBe(originalUpdatedAt);
    }

    [Fact]
    public void Given_ExistingPatient_When_UpdatedFromRequest_Then_CreatedAtIsPreserved()
    {
        // Arrange
        var originalCreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var patient = new Patient
        {
            Id = "507f1f77bcf86cd799439011",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com",
            CreatedAt = originalCreatedAt,
            UpdatedAt = originalCreatedAt
        };

        var request = new UpdatePatientRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com"
        };

        // Act
        patient.UpdateFrom(request);

        // Assert
        patient.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void Given_ExistingPatient_When_UpdatedFromRequest_Then_IdIsPreserved()
    {
        // Arrange
        var originalId = "507f1f77bcf86cd799439011";
        var patient = new Patient
        {
            Id = originalId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com"
        };

        var request = new UpdatePatientRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "jonathan.doe@example.com"
        };

        // Act
        patient.UpdateFrom(request);

        // Assert
        patient.Id.Should().Be(originalId);
    }
}
