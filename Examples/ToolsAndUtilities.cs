using DopplrHub;
using System.Text.Json;

using var api = new DopplrHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var rates = await api.Utilities.CurrencyRatesAsync("USD");
Console.WriteLine(rates["base"]);

var ocrJob = await api.Tools.OcrAsync("./input.pdf", "ocr-docx", language: "eng");
await ocrJob.WaitAsync();
await ocrJob.DownloadAsync("./input.docx");

var pdfJob = await api.Tools.PdfAsync(
	"./packet.pdf",
	"compress",
	JsonSerializer.SerializeToNode(new { level = "screen" }));
await pdfJob.WaitAsync();
await pdfJob.DownloadAsync("./packet-compressed.pdf");

var imageJob = await api.Tools.ImageAsync(
	"./image.png",
	"resize",
	JsonSerializer.SerializeToNode(new
	{
		width = 1920,
		height = 1080,
		fit = "cover",
		outputFormat = "webp"
	}));
await imageJob.WaitAsync();
await imageJob.DownloadAsync("./image.webp");

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
