using MinimalApi.Data.Entities;
using MinimalApi.Models;

namespace MinimalApi.Services;

public interface IConversationService
{
    Task<Conversation?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatTurn>> GetHistoryAsync(
        Guid conversationId,
        int maxMessages,
        CancellationToken cancellationToken = default
    );

    Task<Conversation> CreateAsync(
        string title,
        string model,
        CancellationToken cancellationToken = default
    );

    Task AppendExchangeAsync(
        Guid conversationId,
        string userMessage,
        string answer,
        string model,
        long latencyMs,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ConversationSummary>> ListAsync(
        CancellationToken cancellationToken = default
    );

    Task<ConversationDetail?> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    );

    Task<bool> DeleteAsync(Guid conversationId, CancellationToken cancellationToken = default);
}
