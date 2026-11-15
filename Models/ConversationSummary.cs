namespace MinimalApi.Models;

public sealed record ConversationSummary(
    Guid Id,
    string Title,
    string Model,
    DateTimeOffset UpdatedAt,
    int MessageCount
);
