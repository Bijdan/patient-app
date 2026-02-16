namespace PatientApp.Application.DTOs;

public class HealthLinkRetrievalResult
{
    public bool Found { get; set; }
    public bool Expired { get; set; }
    public string? JweCompactSerialization { get; set; }
}
