using PatientApp.Application.DTOs;

namespace PatientApp.Application.Interfaces;

public interface IHealthLinkService
{
    Task<SmartHealthLinkDto> ProcessBundleAsync(string bundleJson, string baseUrl);
    Task<HealthLinkRetrievalResult> RetrieveAsync(string submissionId, string recipient);
}
