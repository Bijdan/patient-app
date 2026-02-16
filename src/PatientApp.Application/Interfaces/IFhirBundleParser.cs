using PatientApp.Application.DTOs;

namespace PatientApp.Application.Interfaces;

public interface IFhirBundleParser
{
    FhirBundleParseResult Parse(string bundleJson);
}
