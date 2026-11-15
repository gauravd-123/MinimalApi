namespace MinimalApi.Services;

public sealed class OllamaUnavailableException : Exception
{
    public OllamaUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

public sealed class OllamaTimeoutException : Exception
{
    public OllamaTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

public sealed class OllamaResponseException : Exception
{
    public OllamaResponseException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
