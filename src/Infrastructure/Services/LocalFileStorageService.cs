using Core.Interfaces;
using Microsoft.Extensions.Hosting; // 👈 important

namespace Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _env; // 👈 remplace IWebHostEnvironment par IHostEnvironment

    public LocalFileStorageService(IHostEnvironment env)   // 👈 idem ici
    {
        _env = env;
    }

    public async Task<string> SaveAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        // En lib, pas de WebRootPath : on se base sur ContentRootPath/wwwroot
        var wwwroot = Path.Combine(_env.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(wwwroot);

        var folder = Path.Combine(wwwroot, "uploads", "avatars");
        Directory.CreateDirectory(folder);

        var safe = Path.GetFileName(fileName).Replace("..", "").Replace("/", "_");
        var path = Path.Combine(folder, $"{Guid.NewGuid()}_{safe}");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fs, ct);

        return "/uploads/avatars/" + Path.GetFileName(path);
    }

    public Task<string> UploadDocumentAsync(Stream fileStream, string container, string blobName, string contentType, CancellationToken ct)
    {
        // Local fallback — not used in production (Azure Blob is the real implementation)
        return SaveAvatarAsync(fileStream, blobName, contentType, ct);
    }

    public Task<bool> DeleteAsync(string container, string blobName, CancellationToken ct)
    {
        return Task.FromResult(false);
    }
}
