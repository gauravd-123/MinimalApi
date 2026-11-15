namespace MinimalApi.Models;

public sealed record ConversationMessage(
    Guid Id,
    string Role,
    string Content,
    string? Model,
    long LatencyMs,
    DateTimeOffset CreatedAt
);

public sealed record ConversationDetail(
    Guid Id,
    string Title,
    string Model,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ConversationMessage> Messages
);
