using PatientApp.Application.DTOs;
using PatientApp.Domain.Entities;

namespace PatientApp.Application.Mappings;

public static class HealthLinkMappingExtensions
{
    public static SmartHealthLinkDto ToShlDto(
        this HealthLinkSubmission submission,
        string retrievalUrl,
        string base64UrlKey)
    {
        return new SmartHealthLinkDto
        {
            Url = retrievalUrl,
            Flag = "U",
            Key = base64UrlKey,
            Exp = new DateTimeOffset(submission.ExpiresAt, TimeSpan.Zero).ToUnixTimeSeconds(),
            Label = $"{submission.PatientName}'s health summary"
        };
    }
}
