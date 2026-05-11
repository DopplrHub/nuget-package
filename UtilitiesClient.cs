using System.Text.Json.Nodes;

namespace DopplrHub;

public sealed class UtilitiesClient
{
    public UtilitiesClient(DopplrHubClient client)
    {
        Client = client;
    }

    private DopplrHubClient Client { get; }

    public Task<JsonObject> SupportedFormatsAsync(CancellationToken cancellationToken = default)
        => Client.RequestJsonAsync(HttpMethod.Get, "/upload/formats", cancellationToken: cancellationToken);

    public Task<JsonObject> CurrencyRatesAsync(string baseCurrency = "USD", CancellationToken cancellationToken = default)
        => Client.RequestJsonAsync(HttpMethod.Get, $"/tools/units/currency-rates?base={Uri.EscapeDataString(baseCurrency.ToUpperInvariant())}", cancellationToken: cancellationToken);

    public async Task<string> BatchDownloadAsync(IEnumerable<string> jobIds, string targetPath, CancellationToken cancellationToken = default)
    {
        var ids = jobIds.ToList();
        if (ids.Count == 0)
        {
            throw new DopplrHubException("jobIds must be a non-empty array.");
        }

        var response = await Client.SendAsync(
            HttpMethod.Post,
            "/jobs/batch-download",
            new JsonObject
            {
                ["jobIds"] = new JsonArray(ids.Select(id => (JsonNode?)JsonValue.Create(id)).ToArray())
            },
            headers: new Dictionary<string, string> { ["Accept"] = "application/zip" },
            cancellationToken: cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var bodyText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new DopplrHubException($"Batch download failed: {bodyText}", (int)response.StatusCode);
        }

        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var output = File.Create(targetPath);
        await response.Content.CopyToAsync(output, cancellationToken);
        return targetPath;
    }
}
