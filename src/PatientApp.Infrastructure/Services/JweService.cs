using Jose;
using PatientApp.Application.Interfaces;

namespace PatientApp.Infrastructure.Services;

public class JweService : IJweService
{
    public string BuildJweCompactSerialization(byte[] plaintext, byte[] key, string contentType)
    {
        var extraHeaders = new Dictionary<string, object>
        {
            { "cty", contentType }
        };

        return JWE.EncryptBytes(
            plaintext,
            new[] { new JweRecipient(JweAlgorithm.DIR, key) },
            JweEncryption.A256GCM,
            mode: SerializationMode.Compact,
            extraProtectedHeaders: extraHeaders);
    }
}
