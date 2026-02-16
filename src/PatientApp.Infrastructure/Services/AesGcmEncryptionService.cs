using System.Security.Cryptography;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Infrastructure.Services;

public class AesGcmEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public EncryptionResult Encrypt(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        return new EncryptionResult
        {
            Ciphertext = ciphertext,
            Nonce = nonce,
            Tag = tag
        };
    }

    public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] nonce, byte[] tag)
    {
        var plaintext = new byte[ciphertext.Length];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }
}
