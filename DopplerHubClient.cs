using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DopplerHub;

public sealed class DopplerHubClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;

    public DopplerHubClient(string apiKey, string? baseUrl = null, HttpClient? httpClient = null)
    {
        ApiKey = apiKey;
        BaseUrl = (baseUrl ?? "https://api.dopplrhub.com/api/v1").TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();
        Tools = new ToolsClient(this);
        Utilities = new UtilitiesClient(this);
    }

    public string ApiKey { get; }

    public string BaseUrl { get; }

    public ToolsClient Tools { get; }

    public UtilitiesClient Utilities { get; }

    public async Task<UploadedFile> UploadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new DopplerHubException($"Input file not found: {filePath}");
        }

        await using var stream = File.OpenRead(fullPath);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", Path.GetFileName(fullPath));

        return new UploadedFile(await RequestJsonAsync(HttpMethod.Post, "/upload", content: content, cancellationToken: cancellationToken));
    }

    public async Task<UploadedFile> ImportFromUrlAsync(
        string url,
        string? fileName = null,
        string? contentType = null,
        string? authHeader = null,
        CancellationToken cancellationToken = default)
    {
        var payload = FilterNulls(new JsonObject
        {
            ["url"] = url,
            ["fileName"] = fileName ?? DetectRemoteFileName(url),
            ["contentType"] = contentType,
            ["authHeader"] = authHeader,
        });

        return new UploadedFile(await RequestJsonAsync(HttpMethod.Post, "/upload/from-url", payload, cancellationToken: cancellationToken));
    }

    public async Task<ConversionJob> StartAsync(
        string filePath,
        string targetFormat,
        string? originalName = null,
        string? mediaType = null,
        JsonNode? conversionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await UploadAsync(filePath, cancellationToken);
        return await ConvertAsync(upload, targetFormat, originalName, mediaType, conversionSettings, cancellationToken);
    }

    public async Task<ConversionJob> StartFromContentsAsync(
        byte[] contents,
        string fileName,
        string targetFormat,
        CancellationToken cancellationToken = default)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"dopplerhub_{Guid.NewGuid():N}{Path.GetExtension(fileName)}");
        await File.WriteAllBytesAsync(tempFile, contents, cancellationToken);

        try
        {
            var upload = await UploadAsync(tempFile, cancellationToken);
            return await ConvertAsync(upload, targetFormat, fileName, null, null, cancellationToken);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    public async Task<ConversionJob> StartFromUrlAsync(
        string url,
        string targetFormat,
        string? fileName = null,
        string? originalName = null,
        string? contentType = null,
        string? authHeader = null,
        string? mediaType = null,
        JsonNode? conversionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await ImportFromUrlAsync(url, fileName, contentType, authHeader, cancellationToken);
        return await ConvertAsync(upload, targetFormat, originalName ?? fileName, mediaType, conversionSettings, cancellationToken);
    }

    public async Task<ConversionJob> ConvertAsync(
        object source,
        string targetFormat,
        string? originalName = null,
        string? mediaType = null,
        JsonNode? conversionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await NormalizeUploadAsync(source, cancellationToken);
        return await SubmitJobAsync(
            "/convert",
            FilterNulls(new JsonObject
            {
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["targetFormat"] = targetFormat,
                ["originalName"] = originalName ?? upload.FileName,
                ["mediaType"] = mediaType,
                ["conversionSettings"] = conversionSettings,
            }),
            cancellationToken);
    }

    public Task<JsonObject> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
        => RequestJsonAsync(HttpMethod.Get, $"/jobs/{Uri.EscapeDataString(jobId)}", cancellationToken: cancellationToken);

    public Task DeleteJobAsync(string jobId, CancellationToken cancellationToken = default)
        => RequestJsonAsync(HttpMethod.Delete, $"/jobs/{Uri.EscapeDataString(jobId)}", cancellationToken: cancellationToken);

    public async Task DownloadFileAsync(string url, string targetPath, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new DopplerHubException($"Download failed with HTTP {(int)response.StatusCode}.", (int)response.StatusCode);
        }

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var output = File.Create(targetPath);
        await response.Content.CopyToAsync(output, cancellationToken);
    }

    internal async Task<UploadedFile> NormalizeUploadAsync(object source, CancellationToken cancellationToken = default)
    {
        switch (source)
        {
            case UploadedFile uploadedFile:
                return uploadedFile;
            case JsonObject jsonObject when jsonObject["fileId"] is not null && jsonObject["inputKey"] is not null:
                return new UploadedFile(jsonObject);
            case string stringSource when stringSource.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || stringSource.StartsWith("https://", StringComparison.OrdinalIgnoreCase):
                return await ImportFromUrlAsync(stringSource, cancellationToken: cancellationToken);
            case string filePath:
                return await UploadAsync(filePath, cancellationToken);
            default:
                throw new DopplerHubException("Source must be a local file path, remote URL, UploadedFile, or upload response object.");
        }
    }

    internal async Task<IReadOnlyList<UploadedFile>> NormalizeUploadsAsync(IEnumerable<object> sources, CancellationToken cancellationToken = default)
    {
        var list = sources.ToList();
        if (list.Count == 0)
        {
            throw new DopplerHubException("At least one source is required.");
        }

        var uploads = new List<UploadedFile>(list.Count);
        foreach (var source in list)
        {
            uploads.Add(await NormalizeUploadAsync(source, cancellationToken));
        }

        return uploads;
    }

    internal async Task<ConversionJob> SubmitJobAsync(string endpoint, JsonObject payload, CancellationToken cancellationToken = default)
    {
        var response = await RequestJsonAsync(HttpMethod.Post, endpoint, payload, cancellationToken: cancellationToken);
        if (response["originalName"] is null && payload["originalName"] is not null)
        {
            response["originalName"] = payload["originalName"]!.DeepClone();
        }

        return new ConversionJob(this, response);
    }

    internal string ExtensionFromPayload(JsonObject payload)
    {
        var outputKey = payload["outputKey"]?.GetValue<string>()
            ?? payload["reportKey"]?.GetValue<string>()
            ?? payload["optimizedResumeKey"]?.GetValue<string>();

        if (!string.IsNullOrWhiteSpace(outputKey))
        {
            var extension = Path.GetExtension(outputKey).TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(extension))
            {
                return extension;
            }
        }

        return GuessExtension(payload["targetFormat"]?.GetValue<string>() ?? "bin");
    }

    internal static string GuessExtension(string targetFormat)
    {
        var normalized = (targetFormat ?? string.Empty).Trim().ToLowerInvariant();
        var parts = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? normalized : parts[^1];
    }

    internal async Task<JsonObject> RequestJsonAsync(
        HttpMethod method,
        string requestPath,
        JsonNode? jsonPayload = null,
        HttpContent? content = null,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync(method, requestPath, jsonPayload, content, headers, cancellationToken);
        var bodyText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw BuildException(response, bodyText);
        }

        if (string.IsNullOrWhiteSpace(bodyText))
        {
            return new JsonObject();
        }

        try
        {
            return JsonNode.Parse(bodyText)?.AsObject() ?? throw new DopplerHubException($"Expected JSON object for {method} {requestPath}.");
        }
        catch (JsonException ex)
        {
            throw new DopplerHubException($"Expected JSON response for {method} {requestPath}, got: {bodyText}", null, ex);
        }
    }

    internal async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string requestPath,
        JsonNode? jsonPayload = null,
        HttpContent? content = null,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(method, requestPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? requestPath
            : $"{BaseUrl}/{requestPath.TrimStart('/')}");

        request.Headers.Add("x-api-key", ApiKey);
        if (headers is not null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (jsonPayload is not null)
        {
            request.Content = new StringContent(jsonPayload.ToJsonString(JsonOptions), Encoding.UTF8, "application/json");
        }
        else if (content is not null)
        {
            request.Content = content;
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    internal static JsonObject FilterNulls(JsonObject payload)
    {
        var keys = payload.Select(item => item.Key).ToList();
        foreach (var key in keys.Where(key => payload[key] is null))
        {
            payload.Remove(key);
        }

        return payload;
    }

    internal static string DetectRemoteFileName(string url)
    {
        var name = Path.GetFileName(new Uri(url).AbsolutePath);
        return string.IsNullOrWhiteSpace(name) ? "remote-input.bin" : name;
    }

    private static DopplerHubException BuildException(HttpResponseMessage response, string bodyText)
    {
        try
        {
            var error = JsonNode.Parse(bodyText)?["error"]?.GetValue<string>();
            return new DopplerHubException(error ?? $"HTTP {(int)response.StatusCode}", (int)response.StatusCode);
        }
        catch (JsonException)
        {
            return new DopplerHubException($"HTTP {(int)response.StatusCode}", (int)response.StatusCode);
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
