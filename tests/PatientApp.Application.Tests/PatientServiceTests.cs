using FluentAssertions;
using NSubstitute;
using PatientApp.Application.DTOs;
using PatientApp.Application.Services;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Application.Tests;

public class PatientServiceTests
{
    private readonly IPatientRepository _repository;
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        _repository = Substitute.For<IPatientRepository>();
        _sut = new PatientService(_repository);
    }

    private static Patient CreateTestPatient(string id = "507f1f77bcf86cd799439011") => new()
    {
        Id = id,
        FirstName = "John",
        LastName = "Doe",
        DateOfBirth = new DateTime(1985, 3, 15),
        Email = "john.doe@example.com",
        Phone = "+1-555-0101",
        CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc)
    };

    // --- GetAllAsync ---

    [Fact]
    public async Task Given_PatientsExist_When_GetAllAsync_Then_ReturnsMappedDtos()
    {
        // Arrange
        var patients = new List<Patient>
        {
            CreateTestPatient("id1"),
            CreateTestPatient("id2")
        };
        _repository.GetAllAsync().Returns(patients);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be("id1");
        dtos[1].Id.Should().Be("id2");
    }

    [Fact]
    public async Task Given_NoPatientsExist_When_GetAllAsync_Then_ReturnsEmptyList()
    {
        // Arrange
        _repository.GetAllAsync().Returns(Enumerable.Empty<Patient>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task Given_PatientExists_When_GetByIdAsync_Then_ReturnsDto()
    {
        // Arrange
        var patient = CreateTestPatient();
        _repository.GetByIdAsync(patient.Id).Returns(patient);

        // Act
        var result = await _sut.GetByIdAsync(patient.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(patient.Id);
        result.FirstName.Should().Be(patient.FirstName);
        result.Email.Should().Be(patient.Email);
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_GetByIdAsync_Then_ReturnsNull()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((Patient?)null);

        // Act
        var result = await _sut.GetByIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task Given_ValidRequest_When_CreateAsync_Then_CallsRepositoryAndReturnsDto()
    {
        // Arrange
        var request = new CreatePatientRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 7, 22),
            Email = "jane.smith@example.com",
            Phone = "+1-555-0102"
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane.smith@example.com");
        await _repository.Received(1).CreateAsync(Arg.Is<Patient>(p =>
            p.FirstName == "Jane" && p.Email == "jane.smith@example.com"));
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task Given_PatientExists_When_UpdateAsync_Then_UpdatesAndReturnsDto()
    {
        // Arrange
        var patient = CreateTestPatient();
        _repository.GetByIdAsync(patient.Id).Returns(patient);

        var request = new UpdatePatientRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe-Updated",
            DateOfBirth = new DateTime(1985, 3, 16),
            Email = "jonathan.doe@example.com",
            Phone = "+1-555-9999"
        };

        // Act
        var result = await _sut.UpdateAsync(patient.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jonathan");
        result.LastName.Should().Be("Doe-Updated");
        result.Email.Should().Be("jonathan.doe@example.com");
        await _repository.Received(1).UpdateAsync(Arg.Any<Patient>());
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_UpdateAsync_Then_ReturnsNull()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((Patient?)null);
        var request = new UpdatePatientRequest
        {
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow,
            Email = "test@example.com"
        };

        // Act
        var result = await _sut.UpdateAsync("nonexistent", request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_UpdateAsync_Then_DoesNotCallRepositoryUpdate()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((Patient?)null);
        var request = new UpdatePatientRequest
        {
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow,
            Email = "test@example.com"
        };

        // Act
        await _sut.UpdateAsync("nonexistent", request);

        // Assert
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Patient>());
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task Given_PatientExists_When_DeleteAsync_Then_ReturnsTrue()
    {
        // Arrange
        var patient = CreateTestPatient();
        _repository.GetByIdAsync(patient.Id).Returns(patient);

        // Act
        var result = await _sut.DeleteAsync(patient.Id);

        // Assert
        result.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(patient.Id);
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_DeleteAsync_Then_ReturnsFalse()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((Patient?)null);

        // Act
        var result = await _sut.DeleteAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_DeleteAsync_Then_DoesNotCallRepositoryDelete()
    {
        // Arrange
        _repository.GetByIdAsync("nonexistent").Returns((Patient?)null);

        // Act
        await _sut.DeleteAsync("nonexistent");

        // Assert
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<string>());
    }
}
