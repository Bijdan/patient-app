using PatientApp.Domain.Common;

namespace PatientApp.Domain.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }
}
