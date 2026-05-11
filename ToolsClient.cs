using System.Text.Json.Nodes;

namespace DopplrHub;

public sealed class ToolsClient
{
    public ToolsClient(DopplrHubClient client)
    {
        Client = client;
    }

    private DopplrHubClient Client { get; }

    public Task<ConversionJob> PdfMergeAsync(
        IEnumerable<object> sources,
        JsonNode? parameters = null,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(sources.ToArray(), "merge", parameters, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfSplitAsync(
        object source,
        string ranges = "",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "split", new JsonObject { ["ranges"] = ranges }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfCompressAsync(
        object source,
        string quality = "medium",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "compress", new JsonObject { ["quality"] = quality }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfRotateAsync(
        object source,
        int degrees = 90,
        string pages = "all",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "rotate", new JsonObject
        {
            ["degrees"] = degrees,
            ["pages"] = pages,
        }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfProtectAsync(
        object source,
        string userPassword,
        string ownerPassword = "",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "protect", new JsonObject
        {
            ["userPassword"] = userPassword,
            ["ownerPassword"] = ownerPassword,
        }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfUnlockAsync(
        object source,
        string password,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "unlock", new JsonObject { ["password"] = password }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfFlattenAsync(
        object source,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "flatten", new JsonObject(), originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfResizeAsync(
        object source,
        int? width = null,
        int? height = null,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "resize", DopplrHubClient.FilterNulls(new JsonObject
        {
            ["width"] = width,
            ["height"] = height,
        }), originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfCropAsync(
        object source,
        int left,
        int top,
        int width,
        int height,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "crop", new JsonObject
        {
            ["left"] = left,
            ["top"] = top,
            ["width"] = width,
            ["height"] = height,
        }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfOrganizeAsync(
        object source,
        string pages = "",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "organize", new JsonObject { ["pages"] = pages }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfExtractImagesAsync(
        object source,
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "extract-images", new JsonObject(), originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfRemovePagesAsync(
        object source,
        string pages = "",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "remove-pages", new JsonObject { ["pages"] = pages }, originalName, cancellationToken: cancellationToken);

    public Task<ConversionJob> PdfExtractPagesAsync(
        object source,
        string ranges = "",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => PdfAsync(source, "extract-pages", new JsonObject { ["ranges"] = ranges }, originalName, cancellationToken: cancellationToken);

    public async Task<ConversionJob> SocialResizeAsync(
        object source,
        string platform,
        IEnumerable<string> selectedSizeIds,
        string outputFormat = "jpg",
        string? originalName = null,
        JsonObject? offsets = null,
        long? fileSizeBytes = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        return await Client.SubmitJobAsync(
            "/tools/social-resize",
            DopplrHubClient.FilterNulls(new JsonObject
            {
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["originalName"] = originalName ?? upload.FileName,
                ["platform"] = platform,
                ["selectedSizeIds"] = new JsonArray(selectedSizeIds.Select(id => (JsonNode?)JsonValue.Create(id)).ToArray()),
                ["outputFormat"] = outputFormat,
                ["offsets"] = offsets,
                ["fileSizeBytes"] = fileSizeBytes,
            }),
            cancellationToken);
    }

    public Task<ConversionJob> ImageResizeAsync(
        object source,
        int? width = null,
        int? height = null,
        string fit = "inside",
        string outputFormat = "jpg",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => ImageAsync(source, "resize", DopplrHubClient.FilterNulls(new JsonObject
        {
            ["width"] = width,
            ["height"] = height,
            ["fit"] = fit,
            ["outputFormat"] = outputFormat,
        }), originalName, cancellationToken);

    public Task<ConversionJob> ImageCropAsync(
        object source,
        int left,
        int top,
        int width,
        int height,
        string outputFormat = "jpg",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => ImageAsync(source, "crop", new JsonObject
        {
            ["left"] = left,
            ["top"] = top,
            ["width"] = width,
            ["height"] = height,
            ["outputFormat"] = outputFormat,
        }, originalName, cancellationToken);

    public Task<ConversionJob> ImageRotateAsync(
        object source,
        int angle = 90,
        string outputFormat = "jpg",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => ImageAsync(source, "rotate", new JsonObject
        {
            ["angle"] = angle,
            ["outputFormat"] = outputFormat,
        }, originalName, cancellationToken);

    public Task<ConversionJob> ImageFlipAsync(
        object source,
        string direction = "horizontal",
        string outputFormat = "jpg",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => ImageAsync(source, "flip", new JsonObject
        {
            ["direction"] = direction,
            ["outputFormat"] = outputFormat,
        }, originalName, cancellationToken);

    public Task<ConversionJob> ImageUpscaleAsync(
        object source,
        double scale = 2,
        int? width = null,
        int? height = null,
        string outputFormat = "jpg",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => ImageAsync(source, "upscale", DopplrHubClient.FilterNulls(new JsonObject
        {
            ["scale"] = scale,
            ["width"] = width,
            ["height"] = height,
            ["outputFormat"] = outputFormat,
        }), originalName, cancellationToken);

    public Task<ConversionJob> VideoTrimAsync(
        object source,
        double startTime = 0,
        double? endTime = null,
        string outputFormat = "mp4",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => VideoAsync(source, "trim", new JsonObject
        {
            ["outputFormat"] = outputFormat,
            ["trim"] = DopplrHubClient.FilterNulls(new JsonObject
            {
                ["enabled"] = true,
                ["startTime"] = startTime,
                ["endTime"] = endTime,
            }),
        }, originalName, cancellationToken);

    public Task<ConversionJob> VideoExtractAsync(
        object source,
        double startTime = 0,
        double? endTime = null,
        string outputFormat = "mp4",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => VideoAsync(source, "extract", new JsonObject
        {
            ["outputFormat"] = outputFormat,
            ["trim"] = DopplrHubClient.FilterNulls(new JsonObject
            {
                ["enabled"] = true,
                ["startTime"] = startTime,
                ["endTime"] = endTime,
            }),
        }, originalName, cancellationToken);

    public Task<ConversionJob> VideoCropAsync(
        object source,
        int left,
        int top,
        int width,
        int height,
        string outputFormat = "mp4",
        string? originalName = null,
        CancellationToken cancellationToken = default)
        => VideoAsync(source, "crop", new JsonObject
        {
            ["left"] = left,
            ["top"] = top,
            ["width"] = width,
            ["height"] = height,
            ["outputFormat"] = outputFormat,
        }, originalName, cancellationToken);

    public async Task<ConversionJob> OcrAsync(
        object source,
        string targetFormat = "ocr-pdf",
        string language = "eng",
        string? originalName = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        return await Client.SubmitJobAsync(
            "/tools/ocr",
            new JsonObject
            {
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["targetFormat"] = targetFormat,
                ["originalName"] = originalName ?? upload.FileName,
                ["language"] = language,
            },
            cancellationToken);
    }

    public async Task<ConversionJob> PdfAsync(
        object source,
        string operation,
        JsonNode? parameters = null,
        string? originalName = null,
        IEnumerable<object>? sources = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new JsonObject
        {
            ["operation"] = operation,
            ["params"] = parameters ?? new JsonObject(),
        };

        if (string.Equals(operation, "merge", StringComparison.OrdinalIgnoreCase))
        {
            var mergeSources = source as IEnumerable<object> ?? sources;
            if (mergeSources is null)
            {
                throw new DopplrHubException("PDF merge requires an array of sources.");
            }

            var uploads = await Client.NormalizeUploadsAsync(mergeSources, cancellationToken);
            payload["fileId"] = uploads[0].FileId;
            payload["inputKeys"] = new JsonArray(uploads.Select(item => (JsonNode?)JsonValue.Create(item.InputKey)).ToArray());
            payload["inputKey"] = uploads[0].InputKey;
            payload["originalName"] = originalName ?? uploads[0].FileName;
        }
        else
        {
            var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
            payload["fileId"] = upload.FileId;
            payload["inputKey"] = upload.InputKey;
            payload["originalName"] = originalName ?? upload.FileName;
        }

        return await Client.SubmitJobAsync("/tools/pdf", payload, cancellationToken);
    }

    public async Task<ConversionJob> ImageAsync(object source, string operation, JsonNode? parameters = null, string? originalName = null, CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        return await Client.SubmitJobAsync(
            "/tools/image",
            new JsonObject
            {
                ["operation"] = operation,
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["originalName"] = originalName ?? upload.FileName,
                ["params"] = parameters ?? new JsonObject(),
            },
            cancellationToken);
    }

    public async Task<ConversionJob> VideoAsync(object source, string operation, JsonNode? parameters = null, string? originalName = null, CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        return await Client.SubmitJobAsync(
            "/tools/video",
            new JsonObject
            {
                ["operation"] = operation,
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["originalName"] = originalName ?? upload.FileName,
                ["params"] = parameters ?? new JsonObject(),
            },
            cancellationToken);
    }

    public async Task<ConversionJob> ArchiveAsync(
        IEnumerable<object> sources,
        string targetFormat = "zip",
        string archiveName = "archive",
        string inputPassword = "",
        string outputPassword = "",
        CancellationToken cancellationToken = default)
    {
        var uploads = await Client.NormalizeUploadsAsync(sources, cancellationToken);
        return await Client.SubmitJobAsync(
            "/tools/archive",
            new JsonObject
            {
                ["inputKeys"] = new JsonArray(uploads.Select(item => (JsonNode?)JsonValue.Create(item.InputKey)).ToArray()),
                ["fileNames"] = new JsonArray(uploads.Select(item => (JsonNode?)JsonValue.Create(item.FileName)).ToArray()),
                ["targetFormat"] = targetFormat,
                ["archiveName"] = archiveName,
                ["inputPassword"] = inputPassword,
                ["outputPassword"] = outputPassword,
            },
            cancellationToken);
    }

    public async Task<ImmediateResult> AdaAsync(object source, string? originalName = null, string? contentType = null, CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        var response = await Client.RequestJsonAsync(
            HttpMethod.Post,
            "/tools/ada/analyze",
            DopplrHubClient.FilterNulls(new JsonObject
            {
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["originalName"] = originalName ?? upload.FileName,
                ["contentType"] = contentType,
            }),
            cancellationToken: cancellationToken);

        return new ImmediateResult(Client, response, "reportDownloadUrl", "reportKey");
    }

    public async Task<ImmediateResult> AtsAsync(
        object source,
        string jobDescription,
        string? originalName = null,
        string? contentType = null,
        string? industry = null,
        string? templateId = null,
        CancellationToken cancellationToken = default)
    {
        var upload = await Client.NormalizeUploadAsync(source, cancellationToken);
        var response = await Client.RequestJsonAsync(
            HttpMethod.Post,
            "/tools/ats/analyze",
            DopplrHubClient.FilterNulls(new JsonObject
            {
                ["fileId"] = upload.FileId,
                ["inputKey"] = upload.InputKey,
                ["originalName"] = originalName ?? upload.FileName,
                ["contentType"] = contentType,
                ["jobDescription"] = jobDescription,
                ["industry"] = industry,
                ["templateId"] = templateId,
            }),
            cancellationToken: cancellationToken);

        return new ImmediateResult(Client, response, "optimizedResumeDownloadUrl", "optimizedResumeKey");
    }

    public async Task<ImmediateResult> AtsReexportAsync(
        JsonObject report,
        string templateId,
        string? fileId = null,
        string? originalName = null,
        string downloadAs = "optimized-resume.docx",
        CancellationToken cancellationToken = default)
    {
        var response = await Client.RequestJsonAsync(
            HttpMethod.Post,
            "/tools/ats/reexport",
            DopplrHubClient.FilterNulls(new JsonObject
            {
                ["report"] = report,
                ["templateId"] = templateId,
                ["fileId"] = fileId,
                ["originalName"] = originalName,
            }),
            cancellationToken: cancellationToken);

        return new ImmediateResult(Client, response, "optimizedResumeDownloadUrl", null, downloadAs);
    }
}
