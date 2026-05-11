using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.StartFromUrlAsync("https://example.com/sample.pdf", "png");
await job.WaitAsync();
await job.DownloadAsync("./sample.png");
await job.DeleteAsync();

var ocrJob = await api.Tools.OcrAsync(
	"https://example.com/scanned-contract.pdf",
	"ocr-docx",
	language: "eng");
await ocrJob.WaitAsync();
await ocrJob.DownloadAsync("./scanned-contract.docx");
await ocrJob.DeleteAsync();

var atsResult = await api.Tools.AtsAsync(
	"https://example.com/resume.pdf",
	"Senior .NET engineer with API design and automation experience",
	industry: "technology");
await atsResult.DownloadAsync("./resume-optimized.docx");
