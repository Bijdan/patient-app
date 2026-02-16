namespace PatientApp.Application.DTOs;

public class FhirBundleParseResult
{
    public string BundleJson { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public byte[] PdfBytes { get; set; } = null!;
}
