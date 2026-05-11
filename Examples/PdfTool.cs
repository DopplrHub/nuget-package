using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.Tools.PdfCompressAsync("./packet.pdf", "screen");
await job.WaitAsync();
await job.DownloadAsync("./packet-compressed.pdf");
