namespace DopplrHub;

public sealed class DopplrHubException : Exception
{
    public DopplrHubException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
