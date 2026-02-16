using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using PatientApp.Application.DTOs;
using PatientApp.Application.Interfaces;

namespace PatientApp.Infrastructure.Services;

public class FhirBundleParser : IFhirBundleParser
{
    public FhirBundleParseResult Parse(string bundleJson)
    {
        var parser = new FhirJsonParser();
        var bundle = parser.Parse<Bundle>(bundleJson);

        if (bundle.Type != Bundle.BundleType.Collection)
            throw new ArgumentException("Bundle must be of type 'collection'.");

        // Extract Patient resource
        var patientResource = bundle.Entry
            .Select(e => e.Resource)
            .OfType<Patient>()
            .FirstOrDefault()
            ?? throw new ArgumentException("Bundle must contain a Patient resource.");

        var patientName = patientResource.Name?.FirstOrDefault();
        var displayName = patientName is not null
            ? $"{patientName.Given?.FirstOrDefault()} {patientName.Family}".Trim()
            : "Unknown Patient";

        // Extract DocumentReference resource
        var docRef = bundle.Entry
            .Select(e => e.Resource)
            .OfType<DocumentReference>()
            .FirstOrDefault()
            ?? throw new ArgumentException("Bundle must contain a DocumentReference resource.");

        var attachment = docRef.Content?.FirstOrDefault()?.Attachment
            ?? throw new ArgumentException("DocumentReference must have content with an attachment.");

        if (attachment.ContentType != "application/pdf")
            throw new ArgumentException("Attachment contentType must be 'application/pdf'.");

        var pdfBytes = attachment.Data
            ?? throw new ArgumentException("Attachment must contain base64-encoded data.");

        return new FhirBundleParseResult
        {
            BundleJson = bundleJson,
            PatientName = displayName,
            PdfBytes = pdfBytes
        };
    }
}
