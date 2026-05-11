using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var job = await api.Tools.VideoTrimAsync("./clip.mp4", startTime: 3, endTime: 12, outputFormat: "mp4");
await job.WaitAsync();
await job.DownloadAsync("./clip-trimmed.mp4");
