namespace DopplerHub;

public sealed class DopplerHubException : Exception
{
    public DopplerHubException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
