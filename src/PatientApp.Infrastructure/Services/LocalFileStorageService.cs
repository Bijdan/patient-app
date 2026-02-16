using PatientApp.Application.Interfaces;
using PatientApp.Infrastructure.Settings;

namespace PatientApp.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(FileStorageSettings settings)
    {
        _basePath = settings.BasePath;
    }

    public async Task WriteFileAsync(string relativePath, byte[] data)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(fullPath, data);
    }

    public async Task<byte[]> ReadFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_basePath, relativePath);
        return await File.ReadAllBytesAsync(fullPath);
    }
}
