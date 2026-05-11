using DopplrHub;

using var api = new DopplrHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.StartAsync("./input.pdf", "jpg");
await job.WaitAsync();
await job.DownloadAsync("./input.jpg");
await job.DeleteAsync();

var pdfJob = await api.Tools.PdfCompressAsync("./packet.pdf", quality: "screen");
await pdfJob.WaitAsync();
await pdfJob.DownloadAsync("./packet-compressed.pdf");
await pdfJob.DeleteAsync();

var adaReport = await api.Tools.AdaAsync("./brochure.pdf");
await adaReport.DownloadAsync("./brochure-ada-report.pdf");
