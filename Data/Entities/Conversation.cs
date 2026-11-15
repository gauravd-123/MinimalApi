namespace MinimalApi.Data.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
