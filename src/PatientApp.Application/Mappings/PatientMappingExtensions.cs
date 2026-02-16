using PatientApp.Application.DTOs;
using PatientApp.Domain.Entities;

namespace PatientApp.Application.Mappings;

public static class PatientMappingExtensions
{
    public static PatientDto ToDto(this Patient patient)
    {
        return new PatientDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth,
            Email = patient.Email,
            Phone = patient.Phone,
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }

    public static Patient ToEntity(this CreatePatientRequest request)
    {
        var now = DateTime.UtcNow;
        return new Patient
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static void UpdateFrom(this Patient patient, UpdatePatientRequest request)
    {
        patient.FirstName = request.FirstName;
        patient.LastName = request.LastName;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Email = request.Email;
        patient.Phone = request.Phone;
        patient.UpdatedAt = DateTime.UtcNow;
    }
}
