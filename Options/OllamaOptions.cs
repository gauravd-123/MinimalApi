namespace MinimalApi.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://127.0.0.1:11434";

    public string ChatModel { get; set; } = "llama3.2:1b";

    public int TimeoutSeconds { get; set; } = 180;
}
