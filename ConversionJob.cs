using System.Text.Json.Nodes;

namespace DopplrHub;

public sealed class ConversionJob
{
    public ConversionJob(DopplrHubClient client, JsonObject payload)
    {
        Client = client;
        Payload = payload;
    }

    internal DopplrHubClient Client { get; }

    public JsonObject Payload { get; private set; }

    public string JobId => Payload["jobId"]?.GetValue<string>() ?? string.Empty;

    public string State => Payload["state"]?.GetValue<string>()
        ?? Payload["status"]?.GetValue<string>()
        ?? "queued";

    public async Task<ConversionJob> RefreshAsync(CancellationToken cancellationToken = default)
    {
        Payload = await Client.GetJobAsync(JobId, cancellationToken);
        return this;
    }

    public async Task<ConversionJob> WaitAsync(int timeoutSeconds = 900, int pollSeconds = 2, CancellationToken cancellationToken = default)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(Math.Max(timeoutSeconds, 1));
        while (DateTimeOffset.UtcNow <= deadline)
        {
            await RefreshAsync(cancellationToken);
            var state = State.ToLowerInvariant();
            if (state == "completed") return this;
            if (state == "failed")
            {
                throw new DopplrHubException(Payload["failedReason"]?.GetValue<string>() ?? "Conversion failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(pollSeconds, 1)), cancellationToken);
        }

        throw new DopplrHubException($"Timed out waiting for conversion job {JobId}");
    }

    public async Task<ConversionJob> DownloadAsync(string? targetPath = null, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(State, "completed", StringComparison.OrdinalIgnoreCase))
        {
            await RefreshAsync(cancellationToken);
        }

        if (!string.Equals(State, "completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new DopplrHubException($"Job {JobId} is not completed.");
        }

        var downloadUrl = Payload["downloadUrl"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            throw new DopplrHubException("Completed job did not include a downloadUrl.");
        }

        await Client.DownloadFileAsync(downloadUrl, targetPath ?? GetDefaultDownloadPath(), cancellationToken);
        return this;
    }

    public async Task<ConversionJob> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Client.DeleteJobAsync(JobId, cancellationToken);
        return this;
    }

    private string GetDefaultDownloadPath()
    {
        var outputKey = Payload["outputKey"]?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(outputKey))
        {
            return Path.Combine(".", Path.GetFileName(outputKey));
        }

        var originalName = Payload["originalName"]?.GetValue<string>() ?? "output";
        var baseName = Path.GetFileNameWithoutExtension(originalName);
        var extension = Client.ExtensionFromPayload(Payload);
        return Path.Combine(".", $"{baseName}.{extension}");
    }
}
