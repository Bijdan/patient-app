using PatientApp.Domain.Entities;

namespace PatientApp.Domain.Interfaces;

public interface IPatientRepository
{
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(string id);
    Task<Patient?> GetByEmailAsync(string email);
    Task CreateAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(string id);
}
