namespace PatientApp.Domain.Common;

public abstract class BaseEntity
{
    public string Id { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
