using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.Tools.OcrAsync("./scan.pdf", "ocr-docx", language: "eng");
await job.WaitAsync();
await job.DownloadAsync("./scan.docx");
