using DopplrHub;

using var api = new DopplrHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var report = await api.Tools.AdaAsync("./brochure.pdf");
await report.DownloadAsync("./brochure-ada-report.pdf");
