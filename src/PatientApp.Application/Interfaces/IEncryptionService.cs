using PatientApp.Application.DTOs;

namespace PatientApp.Application.Interfaces;

public interface IEncryptionService
{
    EncryptionResult Encrypt(byte[] plaintext, byte[] key);
    byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] nonce, byte[] tag);
}
