namespace MinimalApi.Models;

public sealed record ChatResponse(string Answer, string Model, Guid ConversationId);
