namespace PatientApp.Application.DTOs;

public class UpdatePatientRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
}
