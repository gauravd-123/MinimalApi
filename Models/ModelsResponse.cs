namespace MinimalApi.Models;

public sealed record ModelsResponse(IReadOnlyList<string> Models, string DefaultModel);
