namespace Core.Interfaces;
public interface IFileStorageService {
  Task<string> SaveAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct);
  Task<string> UploadDocumentAsync(Stream fileStream, string container, string blobName, string contentType, CancellationToken ct);
  Task<bool> DeleteAsync(string container, string blobName, CancellationToken ct);
}
