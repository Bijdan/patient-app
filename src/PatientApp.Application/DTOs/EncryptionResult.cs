namespace PatientApp.Application.DTOs;

public class EncryptionResult
{
    public byte[] Ciphertext { get; set; } = null!;
    public byte[] Nonce { get; set; } = null!;
    public byte[] Tag { get; set; } = null!;
}
