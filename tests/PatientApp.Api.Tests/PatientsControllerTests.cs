using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PatientApp.Api.Controllers;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Api.Tests;

public class PatientsControllerTests
{
    private readonly IPatientService _patientService;
    private readonly PatientsController _sut;

    public PatientsControllerTests()
    {
        _patientService = Substitute.For<IPatientService>();
        _sut = new PatientsController(_patientService);
    }

    private static PatientDto CreateTestDto(string id = "507f1f77bcf86cd799439011") => new()
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

    // --- GetAll ---

    [Fact]
    public async Task Given_PatientsExist_When_GetAll_Then_Returns200OkWithList()
    {
        // Arrange
        var patients = new List<PatientDto> { CreateTestDto("id1"), CreateTestDto("id2") };
        _patientService.GetAllAsync().Returns(patients);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
        returnedPatients.Should().HaveCount(2);
    }

    [Fact]
    public async Task Given_NoPatientsExist_When_GetAll_Then_Returns200OkWithEmptyList()
    {
        // Arrange
        _patientService.GetAllAsync().Returns(Enumerable.Empty<PatientDto>());

        // Act
        var result = await _sut.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPatients = okResult.Value.Should().BeAssignableTo<IEnumerable<PatientDto>>().Subject;
        returnedPatients.Should().BeEmpty();
    }

    // --- GetById ---

    [Fact]
    public async Task Given_PatientExists_When_GetById_Then_Returns200Ok()
    {
        // Arrange
        var dto = CreateTestDto();
        _patientService.GetByIdAsync(dto.Id).Returns(dto);

        // Act
        var result = await _sut.GetById(dto.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPatient = okResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_GetById_Then_Returns404NotFound()
    {
        // Arrange
        _patientService.GetByIdAsync("nonexistent").Returns((PatientDto?)null);

        // Act
        var result = await _sut.GetById("nonexistent");

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- Create ---

    [Fact]
    public async Task Given_ValidRequest_When_Create_Then_Returns201Created()
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

        var createdDto = new PatientDto
        {
            Id = "new-id-123",
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1990, 7, 22),
            Email = "jane.smith@example.com",
            Phone = "+1-555-0102",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _patientService.CreateAsync(request).Returns(createdDto);

        // Act
        var result = await _sut.Create(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(PatientsController.GetById));
        createdResult.RouteValues!["id"].Should().Be("new-id-123");
        var returnedPatient = createdResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.FirstName.Should().Be("Jane");
    }

    // --- Update ---

    [Fact]
    public async Task Given_PatientExists_When_Update_Then_Returns200Ok()
    {
        // Arrange
        var id = "507f1f77bcf86cd799439011";
        var request = new UpdatePatientRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe-Updated",
            DateOfBirth = new DateTime(1985, 3, 16),
            Email = "jonathan.doe@example.com",
            Phone = "+1-555-9999"
        };

        var updatedDto = new PatientDto
        {
            Id = id,
            FirstName = "Jonathan",
            LastName = "Doe-Updated",
            DateOfBirth = new DateTime(1985, 3, 16),
            Email = "jonathan.doe@example.com",
            Phone = "+1-555-9999",
            CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = DateTime.UtcNow
        };

        _patientService.UpdateAsync(id, request).Returns(updatedDto);

        // Act
        var result = await _sut.Update(id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedPatient = okResult.Value.Should().BeOfType<PatientDto>().Subject;
        returnedPatient.FirstName.Should().Be("Jonathan");
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_Update_Then_Returns404NotFound()
    {
        // Arrange
        var request = new UpdatePatientRequest
        {
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.UtcNow,
            Email = "test@example.com"
        };
        _patientService.UpdateAsync("nonexistent", request).Returns((PatientDto?)null);

        // Act
        var result = await _sut.Update("nonexistent", request);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // --- Delete ---

    [Fact]
    public async Task Given_PatientExists_When_Delete_Then_Returns204NoContent()
    {
        // Arrange
        _patientService.DeleteAsync("existing-id").Returns(true);

        // Act
        var result = await _sut.Delete("existing-id");

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Given_PatientDoesNotExist_When_Delete_Then_Returns404NotFound()
    {
        // Arrange
        _patientService.DeleteAsync("nonexistent").Returns(false);

        // Act
        var result = await _sut.Delete("nonexistent");

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
