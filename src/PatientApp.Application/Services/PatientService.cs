using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;
using PatientApp.Application.Mappings;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Application.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PatientDto>> GetAllAsync()
    {
        var patients = await _repository.GetAllAsync();
        return patients.Select(p => p.ToDto());
    }

    public async Task<PatientDto?> GetByIdAsync(string id)
    {
        var patient = await _repository.GetByIdAsync(id);
        return patient?.ToDto();
    }

    public async Task<PatientDto> CreateAsync(CreatePatientRequest request)
    {
        var patient = request.ToEntity();
        await _repository.CreateAsync(patient);
        return patient.ToDto();
    }

    public async Task<PatientDto?> UpdateAsync(string id, UpdatePatientRequest request)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return null;

        patient.UpdateFrom(request);
        await _repository.UpdateAsync(patient);
        return patient.ToDto();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var patient = await _repository.GetByIdAsync(id);
        if (patient is null)
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }
}
