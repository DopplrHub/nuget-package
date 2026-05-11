using System.Text.Json.Nodes;

namespace DopplerHub;

public sealed class ImmediateResult
{
    public ImmediateResult(DopplerHubClient client, JsonObject payload, string downloadUrlField, string? downloadKeyField = null, string? defaultFileName = null)
    {
        Client = client;
        Payload = payload;
        DownloadUrlField = downloadUrlField;
        DownloadKeyField = downloadKeyField;
        DefaultFileName = defaultFileName;
    }

    private DopplerHubClient Client { get; }

    public JsonObject Payload { get; }

    private string DownloadUrlField { get; }

    private string? DownloadKeyField { get; }

    private string? DefaultFileName { get; }

    public async Task<ImmediateResult> DownloadAsync(string? targetPath = null, CancellationToken cancellationToken = default)
    {
        var downloadUrl = Payload[DownloadUrlField]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            throw new DopplerHubException("Response did not include a download URL.");
        }

        await Client.DownloadFileAsync(downloadUrl, targetPath ?? GetDefaultDownloadPath(), cancellationToken);
        return this;
    }

    private string GetDefaultDownloadPath()
    {
        if (!string.IsNullOrWhiteSpace(DownloadKeyField))
        {
            var downloadKey = Payload[DownloadKeyField!]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(downloadKey))
            {
                return Path.Combine(".", Path.GetFileName(downloadKey));
            }
        }

        if (!string.IsNullOrWhiteSpace(DefaultFileName))
        {
            return Path.Combine(".", DefaultFileName!);
        }

        var originalName = Payload["originalName"]?.GetValue<string>() ?? "download";
        return Path.Combine(".", $"{Path.GetFileNameWithoutExtension(originalName)}.bin");
    }
}
