namespace PatientApp.Application.Interfaces;

public interface IJweService
{
    string BuildJweCompactSerialization(byte[] plaintext, byte[] key, string contentType);
}
