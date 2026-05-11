using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.Tools.ImageResizeAsync(
    "./hero.png",
    width: 1920,
    height: 1080,
    fit: "cover",
    outputFormat: "webp");
await job.WaitAsync();
await job.DownloadAsync("./hero.webp");
