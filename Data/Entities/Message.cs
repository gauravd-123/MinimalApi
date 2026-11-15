namespace MinimalApi.Data.Entities;

public sealed class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Conversation? Conversation { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? Model { get; set; }

    public long LatencyMs { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
