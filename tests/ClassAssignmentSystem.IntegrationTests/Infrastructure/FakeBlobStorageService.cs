using ClassAssignmentSystem.Application.Common.Interfaces;

namespace ClassAssignmentSystem.IntegrationTests.Infrastructure;

public class FakeBlobStorageService : IBlobStorageService
{
    private readonly Dictionary<string, byte[]> _blobs = new(StringComparer.OrdinalIgnoreCase);

    public Task<string> UploadAsync(
        string containerName,
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var memory = new MemoryStream();
        content.CopyTo(memory);
        _blobs[$"{containerName}/{blobName}"] = memory.ToArray();
        return Task.FromResult(blobName);
    }

    public Task<string> GetReadSasUriAsync(
        string containerName,
        string blobName,
        TimeSpan? validFor = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult($"https://fake-storage.test/{containerName}/{blobName}?sas=fake");

    public Task DeleteAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _blobs.Remove($"{containerName}/{blobName}");
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_blobs.ContainsKey($"{containerName}/{blobName}"));
}
