using System.Security.Cryptography;
using System.Text;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;
using PatientApp.Application.Mappings;
using PatientApp.Domain.Entities;
using PatientApp.Domain.Interfaces;

namespace PatientApp.Application.Services;

public class HealthLinkService : IHealthLinkService
{
    private readonly IFhirBundleParser _fhirParser;
    private readonly IEncryptionService _encryptionService;
    private readonly IFileStorageService _fileStorage;
    private readonly IJweService _jweService;
    private readonly IHealthLinkSubmissionRepository _repository;
    private readonly int _defaultExpiryHours;

    public HealthLinkService(
        IFhirBundleParser fhirParser,
        IEncryptionService encryptionService,
        IFileStorageService fileStorage,
        IJweService jweService,
        IHealthLinkSubmissionRepository repository,
        int defaultExpiryHours = 72)
    {
        _fhirParser = fhirParser;
        _encryptionService = encryptionService;
        _fileStorage = fileStorage;
        _jweService = jweService;
        _repository = repository;
        _defaultExpiryHours = defaultExpiryHours;
    }

    public async Task<SmartHealthLinkDto> ProcessBundleAsync(string bundleJson, string baseUrl)
    {
        // 1. Parse the FHIR Bundle
        var parseResult = _fhirParser.Parse(bundleJson);

        // 2. Generate a 32-byte AES key
        var key = RandomNumberGenerator.GetBytes(32);

        // 3. Generate a submission ID
        var submissionId = Guid.NewGuid().ToString();

        // 4. Encrypt the bundle JSON
        var bundleBytes = Encoding.UTF8.GetBytes(parseResult.BundleJson);
        var bundleEncrypted = _encryptionService.Encrypt(bundleBytes, key);

        // 5. Encrypt the PDF
        var pdfEncrypted = _encryptionService.Encrypt(parseResult.PdfBytes, key);

        // 6. Write encrypted files to disk
        var bundlePath = $"{submissionId}/bundle.enc";
        var pdfPath = $"{submissionId}/document.enc";
        await _fileStorage.WriteFileAsync(bundlePath, bundleEncrypted.Ciphertext);
        await _fileStorage.WriteFileAsync(pdfPath, pdfEncrypted.Ciphertext);

        // 7. Build and persist the submission entity
        var now = DateTime.UtcNow;
        var submission = new HealthLinkSubmission
        {
            Id = submissionId,
            PatientName = parseResult.PatientName,
            EncryptionKey = key,
            BundleNonce = bundleEncrypted.Nonce,
            BundleTag = bundleEncrypted.Tag,
            BundleFilePath = bundlePath,
            PdfNonce = pdfEncrypted.Nonce,
            PdfTag = pdfEncrypted.Tag,
            PdfFilePath = pdfPath,
            ExpiresAt = now.AddHours(_defaultExpiryHours),
            CreatedAt = now,
            UpdatedAt = now
        };
        await _repository.CreateAsync(submission);

        // 8. Build the SHL response
        var retrievalUrl = $"{baseUrl.TrimEnd('/')}/api/v1/healthlinks/{submissionId}";
        var base64UrlKey = Base64UrlEncode(key);
        return submission.ToShlDto(retrievalUrl, base64UrlKey);
    }

    public async Task<HealthLinkRetrievalResult> RetrieveAsync(string submissionId, string recipient)
    {
        var submission = await _repository.GetByIdAsync(submissionId);
        if (submission is null)
            return new HealthLinkRetrievalResult { Found = false };

        if (submission.ExpiresAt < DateTime.UtcNow)
            return new HealthLinkRetrievalResult { Found = true, Expired = true };

        // Read the encrypted bundle from disk
        var encryptedBundle = await _fileStorage.ReadFileAsync(submission.BundleFilePath);

        // Decrypt the bundle
        var bundleBytes = _encryptionService.Decrypt(
            encryptedBundle,
            submission.EncryptionKey,
            submission.BundleNonce,
            submission.BundleTag);

        // Build JWE compact serialization for the HTTP response
        var jwe = _jweService.BuildJweCompactSerialization(
            bundleBytes,
            submission.EncryptionKey,
            "application/fhir+json");

        return new HealthLinkRetrievalResult
        {
            Found = true,
            Expired = false,
            JweCompactSerialization = jwe
        };
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
