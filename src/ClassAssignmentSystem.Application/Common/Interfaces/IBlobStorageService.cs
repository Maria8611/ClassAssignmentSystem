namespace ClassAssignmentSystem.Application.Common.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a stream to blob storage under the given container and returns the
    /// generated blob name (path) that should be persisted on the entity.
    /// </summary>
    Task<string> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a short-lived, read-only SAS URL clients can use to download the blob
    /// directly from Azure without proxying the file through the API.
    /// </summary>
    Task<string> GetReadSasUriAsync(
        string containerName,
        string blobName,
        TimeSpan? validFor = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default);
}

