using System.Diagnostics;
using Supabase.Storage;
using Supabase.Storage.Interfaces;

namespace BlazorApp.Services;

public class SbStorageService
{
    private readonly IStorageClient<Supabase.Storage.Bucket, Supabase.Storage.FileObject> _storage;
    private readonly Supabase.Client _client;
    private readonly ILogger<SbStorageService> _logger;

    public SbStorageService(
        Supabase.Client client,
        ILogger<SbStorageService> logger
    )
    {
        _client = client;
        _logger = logger;
        _storage = _client.Storage;
    }

    public async Task<string> UploadFile(String bucketName, Stream streamData, String fileName)
    {
        var bucket = _storage.From(bucketName);

        var dataAsBytes = await StreamToBytesAsync(streamData);

        var fileExtension = fileName.Split(".").Last();
        var userId = _client.Auth.CurrentUser?.Id;

        var saveName = $"{userId}-" + Guid.NewGuid();
        saveName = saveName
            .Replace("/", "_")
            .Replace(" ", "_")
            .Replace(":", "_");
        saveName = saveName + "." + fileExtension;

        return await bucket.Upload(dataAsBytes, saveName);
    }

    public async Task<List<FileObject>?> GetFilesFromBucket(string bucketName)
    {
        var bucket = _storage.From(bucketName);

        if (bucket is null)
            return Enumerable.Empty<FileObject>().ToList();
            // heap is bypassed and there is nothing to garbage collect.

        return await bucket.List();
    }

    public async Task<byte[]> DownloadFile(string bucketName, string fileName)
    {
        var bucket = _storage.From(bucketName);

        if (bucket is null)
            return Array.Empty<byte>();
            // heap is bypassed and there is nothing to garbage collect.

        return await bucket.Download(fileName, (_, f) => Debug.WriteLine($"Download Progress: {f}%"));
    }

    public async Task<List<FileObject>?> DeleteFiles(string bucketName, List<string> pathsList)
    {
        var bucket = _storage.From(bucketName);
        
        return await bucket.Remove(pathsList);
    }

    private async Task<byte[]> StreamToBytesAsync(Stream streamData)
    {
        byte[] bytes;

        using var memoryStream = new MemoryStream();
        await streamData.CopyToAsync(memoryStream);
        bytes = memoryStream.ToArray();

        return bytes;
    }
}