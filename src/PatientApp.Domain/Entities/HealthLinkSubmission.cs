using PatientApp.Domain.Common;

namespace PatientApp.Domain.Entities;

public class HealthLinkSubmission : BaseEntity
{
    public string PatientName { get; set; } = null!;
    public byte[] EncryptionKey { get; set; } = null!;

    // Bundle encryption artifacts
    public byte[] BundleNonce { get; set; } = null!;
    public byte[] BundleTag { get; set; } = null!;
    public string BundleFilePath { get; set; } = null!;

    // PDF encryption artifacts
    public byte[] PdfNonce { get; set; } = null!;
    public byte[] PdfTag { get; set; } = null!;
    public string PdfFilePath { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }
}
