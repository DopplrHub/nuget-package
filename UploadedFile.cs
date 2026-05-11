using System.Text.Json.Nodes;

namespace DopplrHub;

public sealed class UploadedFile
{
    public UploadedFile(JsonObject payload)
    {
        Payload = payload;
    }

    public JsonObject Payload { get; }

    public string FileId => Payload["fileId"]?.GetValue<string>() ?? string.Empty;

    public string InputKey => Payload["inputKey"]?.GetValue<string>() ?? string.Empty;

    public string FileName => Payload["fileName"]?.GetValue<string>() ?? "input.bin";

    public long? FileSize => Payload["fileSize"]?.GetValue<long?>();
}
