using PatientApp.Domain.Entities;

namespace PatientApp.Domain.Interfaces;

public interface IHealthLinkSubmissionRepository
{
    Task CreateAsync(HealthLinkSubmission submission);
    Task<HealthLinkSubmission?> GetByIdAsync(string id);
}
