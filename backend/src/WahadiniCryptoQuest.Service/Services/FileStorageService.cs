using Microsoft.AspNetCore.Hosting;
using WahadiniCryptoQuest.Core.Interfaces;

namespace WahadiniCryptoQuest.Service.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        return Path.Combine("uploads", folder, uniqueFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_environment.WebRootPath, filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return System.Threading.Tasks.Task.CompletedTask;
    }
}