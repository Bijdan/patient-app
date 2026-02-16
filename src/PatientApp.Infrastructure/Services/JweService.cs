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

        return JWE.Encrypt(
            plaintext: plaintext,
            key: key,
            alg: JweAlgorithm.DIR,
            enc: JweEncryption.A256GCM,
            extraHeaders: extraHeaders);
    }
}
