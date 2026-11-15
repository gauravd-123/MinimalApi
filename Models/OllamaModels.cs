namespace MinimalApi.Models;

public sealed record OllamaChatRequest(
    string Model,
    IReadOnlyList<OllamaMessage> Messages,
    bool Stream = false
);

public sealed record OllamaMessage(string Role, string? Content);

public sealed record OllamaChatResponse(string? Model, OllamaMessage? Message, bool Done);

public sealed record OllamaTagsResponse(IReadOnlyList<OllamaModelSummary> Models);

public sealed record OllamaModelSummary(string Name);
