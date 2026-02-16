using PatientApp.Application.DTOs;

namespace PatientApp.Application.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientDto>> GetAllAsync();
    Task<PatientDto?> GetByIdAsync(string id);
    Task<PatientDto> CreateAsync(CreatePatientRequest request);
    Task<PatientDto?> UpdateAsync(string id, UpdatePatientRequest request);
    Task<bool> DeleteAsync(string id);
}
