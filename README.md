# DopplrHub NuGet Package

A .NET SDK for the current DopplrHubpublic API, including generic conversions, tools, and utility endpoints.

## Install

```bash
dotnet add package DopplrHub
```

## Local file conversion

The starter sample in `Examples/ConvertLocalFile.cs` now covers both a basic conversion flow and tool usage, including a queued PDF tool job and an immediate ADA analysis download.

```csharp
using DopplrHub;

using var api = new DopplrHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.StartAsync("./input.pdf", "jpg");
await job.WaitAsync();
await job.DownloadAsync("./input.jpg");
await job.DeleteAsync();
```

## Remote file conversion

The starter sample in `Examples/ConvertRemoteFile.cs` now covers both remote-file conversion and remote tool usage, including OCR and ATS flows against hosted documents.

```csharp
var job = await api.StartFromUrlAsync("https://example.com/sample.pdf", "png");
await job.WaitAsync();
await job.DownloadAsync("./sample.png");
await job.DeleteAsync();
```

## Tools

```csharp
var rates = await api.Utilities.CurrencyRatesAsync("USD");

var ocrJob = await api.Tools.OcrAsync("./scan.pdf", "ocr-docx", language: "eng");
await ocrJob.WaitAsync();
await ocrJob.DownloadAsync("./scan.docx");

var pdfJob = await api.Tools.PdfAsync(
    "./packet.pdf",
    "compress",
    JsonSerializer.SerializeToNode(new { level = "screen" }));
await pdfJob.WaitAsync();
await pdfJob.DownloadAsync("./packet-compressed.pdf");

var imageJob = await api.Tools.ImageAsync(
    "./hero.png",
    "resize",
    JsonSerializer.SerializeToNode(new
    {
        width = 1920,
        height = 1080,
        fit = "cover",
        outputFormat = "webp"
    }));
await imageJob.WaitAsync();
await imageJob.DownloadAsync("./hero.webp");

var videoJob = await api.Tools.VideoAsync(
    "./clip.mp4",
    "trim",
    JsonSerializer.SerializeToNode(new
    {
        startTime = "00:00:03",
        endTime = "00:00:12",
        outputFormat = "mp4"
    }));
await videoJob.WaitAsync();
await videoJob.DownloadAsync("./clip-trimmed.mp4");

var adaReport = await api.Tools.AdaAsync("./brochure.pdf");
await adaReport.DownloadAsync("./brochure-ada-report.pdf");

var atsResult = await api.Tools.AtsAsync(
    "./resume.pdf",
    "Senior .NET engineer with API design experience",
    industry: "technology");
await atsResult.DownloadAsync("./resume-optimized.docx");

var atsReexported = await api.Tools.AtsReexportAsync(report, "modern", downloadAs: "resume-modern.docx");
await atsReexported.DownloadAsync("./resume-modern.docx");

var archiveJob = await api.Tools.ArchiveAsync(
    new[] { "./a.txt", "./b.txt" },
    targetFormat: "zip",
    archiveName: "documents");
await archiveJob.WaitAsync();
await archiveJob.DownloadAsync("./documents.zip");

var socialJob = await api.Tools.SocialResizeAsync(
    "./hero.png",
    platform: "instagram",
    selectedSizeIds: new[] { "post-square", "story" },
    outputFormat: "jpg");
await socialJob.WaitAsync();
await socialJob.DownloadAsync("./hero-instagram.zip");
```

Tool coverage in the .NET SDK:

- `api.Tools.OcrAsync()` for OCR job flows
- `api.Tools.PdfAsync()` / `api.Tools.PdfMergeAsync()` / `api.Tools.PdfSplitAsync()` / `api.Tools.PdfCompressAsync()` / `api.Tools.PdfRotateAsync()` / `api.Tools.PdfProtectAsync()` / `api.Tools.PdfUnlockAsync()` / `api.Tools.PdfFlattenAsync()` / `api.Tools.PdfResizeAsync()` / `api.Tools.PdfCropAsync()` / `api.Tools.PdfOrganizeAsync()` / `api.Tools.PdfExtractImagesAsync()` / `api.Tools.PdfRemovePagesAsync()` / `api.Tools.PdfExtractPagesAsync()` for PDF workflows
- `api.Tools.ImageAsync()` / `api.Tools.ImageResizeAsync()` / `api.Tools.ImageCropAsync()` / `api.Tools.ImageRotateAsync()` / `api.Tools.ImageFlipAsync()` / `api.Tools.ImageUpscaleAsync()` for image operations
- `api.Tools.VideoAsync()` / `api.Tools.VideoTrimAsync()` / `api.Tools.VideoExtractAsync()` / `api.Tools.VideoCropAsync()` for video job operations
- `api.Tools.ArchiveAsync()` for creating ZIP, 7Z, TAR, TGZ, and TBZ2 archives from multiple uploaded files
- `api.Tools.SocialResizeAsync()` for resizing images into platform-specific dimensions (Facebook, Instagram, Twitter, TikTok, WhatsApp, LinkedIn, YouTube, Pinterest); returns a ZIP when multiple sizes are selected
- `api.Tools.AdaAsync()` for immediate ADA analysis with downloadable reports
- `api.Tools.AtsAsync()` for ATS scoring and optimized resume export
- `api.Tools.AtsReexportAsync()` for re-exporting a previous ATS result with a different template

## Examples

- `Examples/ConvertLocalFile.cs`
- `Examples/ConvertRemoteFile.cs`
- `Examples/OcrTool.cs`
- `Examples/PdfTool.cs`
- `Examples/ImageTool.cs`
- `Examples/VideoTool.cs`
- `Examples/AdaTool.cs`
- `Examples/AtsTool.cs`
- `Examples/ToolsAndUtilities.cs`

## Important behavior note

`StartFromUrlAsync()` currently downloads the remote resource first, then uploads it into DopplrHub.
It does not perform headless browser webpage rendering.
