# DopplerHub NuGet Package

A .NET SDK for the current DopplerHub public API, including generic conversions, tools, and utility endpoints.

## Install

```bash
dotnet add package DopplerHub
```

## Local file conversion

The starter sample in `Examples/ConvertLocalFile.cs` now covers both a basic conversion flow and tool usage, including a queued PDF tool job and an immediate ADA analysis download.

```csharp
using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "http://localhost:3001/api/v1");

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
```

Tool coverage in the .NET SDK:

- `api.Tools.OcrAsync()` for OCR job flows
- `api.Tools.PdfAsync()` for PDF merge, split, compress, protect, unlock, rotate, watermark, and related operations
- `api.Tools.ImageAsync()` for resize, crop, rotate, flip, optimize, and format changes
- `api.Tools.VideoAsync()` for trim, crop, and other video job operations
- `api.Tools.AdaAsync()` for immediate ADA analysis with downloadable reports
- `api.Tools.AtsAsync()` for ATS scoring and optimized resume export

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

`StartFromUrlAsync()` currently downloads the remote resource first, then uploads it into DopplerHub.
It does not perform headless browser webpage rendering.
