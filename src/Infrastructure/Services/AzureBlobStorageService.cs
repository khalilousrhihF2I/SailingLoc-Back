using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Core.Interfaces;

namespace Infrastructure.Services;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> SaveAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var blobName = $"{Guid.NewGuid()}{extension}";
        return await UploadDocumentAsync(fileStream, "profile", blobName, contentType, ct);
    }

    public async Task<string> UploadDocumentAsync(Stream fileStream, string container, string blobName, string contentType, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers }, ct);

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteAsync(string container, string blobName, CancellationToken ct)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        return response.Value;
    }
}
