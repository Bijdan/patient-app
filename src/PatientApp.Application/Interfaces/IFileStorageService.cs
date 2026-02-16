namespace PatientApp.Application.Interfaces;

public interface IFileStorageService
{
    Task WriteFileAsync(string relativePath, byte[] data);
    Task<byte[]> ReadFileAsync(string relativePath);
}
