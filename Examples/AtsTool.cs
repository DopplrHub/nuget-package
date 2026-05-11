using DopplerHub;

using var api = new DopplerHubClient("YOUR_API_KEY", "https://api.dopplrhub.com/api/v1");

var result = await api.Tools.AtsAsync(
    "./resume.pdf",
    "Senior .NET engineer with API design experience",
    industry: "technology");
await result.DownloadAsync("./resume-optimized.docx");
