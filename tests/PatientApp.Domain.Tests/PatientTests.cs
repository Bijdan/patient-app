using FluentAssertions;
using PatientApp.Domain.Common;
using PatientApp.Domain.Entities;

namespace PatientApp.Domain.Tests;

public class PatientTests
{
    [Fact]
    public void Given_NewPatient_When_Created_Then_InheritsFromBaseEntity()
    {
        // Arrange & Act
        var patient = new Patient();

        // Assert
        patient.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Given_NewPatient_When_PropertiesSet_Then_ValuesAreStored()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";
        var now = DateTime.UtcNow;

        // Act
        var patient = new Patient
        {
            Id = id,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1985, 3, 15),
            Email = "john.doe@example.com",
            Phone = "+1-555-0101",
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        patient.Id.Should().Be(id);
        patient.FirstName.Should().Be("John");
        patient.LastName.Should().Be("Doe");
        patient.DateOfBirth.Should().Be(new DateTime(1985, 3, 15));
        patient.Email.Should().Be("john.doe@example.com");
        patient.Phone.Should().Be("+1-555-0101");
        patient.CreatedAt.Should().Be(now);
        patient.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void Given_NewPatient_When_PhoneNotSet_Then_PhoneIsNull()
    {
        // Arrange & Act
        var patient = new Patient
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 7, 22),
            Email = "jane.smith@example.com"
        };

        // Assert
        patient.Phone.Should().BeNull();
    }

    [Fact]
    public void Given_NewPatient_When_Created_Then_HasExpectedProperties()
    {
        // Arrange & Act
        var patientType = typeof(Patient);

        // Assert
        patientType.GetProperty("Id").Should().NotBeNull();
        patientType.GetProperty("FirstName").Should().NotBeNull();
        patientType.GetProperty("LastName").Should().NotBeNull();
        patientType.GetProperty("DateOfBirth").Should().NotBeNull();
        patientType.GetProperty("Email").Should().NotBeNull();
        patientType.GetProperty("Phone").Should().NotBeNull();
        patientType.GetProperty("CreatedAt").Should().NotBeNull();
        patientType.GetProperty("UpdatedAt").Should().NotBeNull();
    }
}
