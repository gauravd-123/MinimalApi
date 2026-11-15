using Microsoft.EntityFrameworkCore;
using MinimalApi.Data;
using MinimalApi.Data.Entities;
using MinimalApi.Models;

namespace MinimalApi.Services;

public sealed class ConversationService : IConversationService
{
    private readonly AppDbContext _db;

    public ConversationService(AppDbContext db)
    {
        _db = db;
    }

    public Task<Conversation?> FindAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return _db.Conversations.FirstOrDefaultAsync(
            conversation => conversation.Id == id,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<ChatTurn>> GetHistoryAsync(
        Guid conversationId,
        int maxMessages,
        CancellationToken cancellationToken = default
    )
    {
        var messages = await _db
            .Messages.Where(message => message.ConversationId == conversationId)
            .OrderByDescending(message => message.CreatedAt)
            .Take(maxMessages)
            .Select(message => new { message.Role, message.Content })
            .ToListAsync(cancellationToken);

        messages.Reverse();

        return messages
            .Select(message => new ChatTurn(message.Role, message.Content))
            .ToList();
    }

    public async Task<Conversation> CreateAsync(
        string title,
        string model,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTimeOffset.UtcNow;

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Title = title,
            Model = model,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync(cancellationToken);

        return conversation;
    }

    public async Task AppendExchangeAsync(
        Guid conversationId,
        string userMessage,
        string answer,
        string model,
        long latencyMs,
        CancellationToken cancellationToken = default
    )
    {
        var conversation =
            await _db.Conversations.FirstOrDefaultAsync(
                item => item.Id == conversationId,
                cancellationToken
            )
            ?? throw new InvalidOperationException(
                $"Conversation '{conversationId}' does not exist."
            );

        var now = DateTimeOffset.UtcNow;

        _db.Messages.AddRange(
            new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = "user",
                Content = userMessage,
                CreatedAt = now,
            },
            new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = "assistant",
                Content = answer,
                Model = model,
                LatencyMs = latencyMs,
                CreatedAt = now.AddMilliseconds(1),
            }
        );

        conversation.Model = model;
        conversation.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSummary>> ListAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _db
            .Conversations.OrderByDescending(conversation => conversation.UpdatedAt)
            .Select(conversation => new ConversationSummary(
                conversation.Id,
                conversation.Title,
                conversation.Model,
                conversation.UpdatedAt,
                conversation.Messages.Count
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationDetail?> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var conversation = await _db
            .Conversations.AsNoTracking()
            .Include(item => item.Messages)
            .FirstOrDefaultAsync(item => item.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        return new ConversationDetail(
            conversation.Id,
            conversation.Title,
            conversation.Model,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation
                .Messages.OrderBy(message => message.CreatedAt)
                .Select(message => new ConversationMessage(
                    message.Id,
                    message.Role,
                    message.Content,
                    message.Model,
                    message.LatencyMs,
                    message.CreatedAt
                ))
                .ToList()
        );
    }

    public async Task<bool> DeleteAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default
    )
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(
            item => item.Id == conversationId,
            cancellationToken
        );

        if (conversation is null)
        {
            return false;
        }

        _db.Conversations.Remove(conversation);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
