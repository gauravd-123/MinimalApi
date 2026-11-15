using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Models;
using MinimalApi.Services;

namespace MinimalApi.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private const int MaxHistoryMessages = 12;

    private readonly IChatService _chatService;
    private readonly IConversationService _conversations;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IConversationService conversations,
        ILogger<ChatController> logger
    )
    {
        _chatService = chatService;
        _conversations = conversations;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<ChatResponse>> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken
    )
    {
        var message = request.Message.Trim();
        if (message.Length == 0)
        {
            return BadRequest(new { error = "Message is required." });
        }

        try
        {
            var conversation =
                request.ConversationId is Guid conversationId
                    ? await _conversations.FindAsync(conversationId, cancellationToken)
                    : null;

            if (request.ConversationId is not null && conversation is null)
            {
                return NotFound(new { error = "Conversation not found." });
            }

            var history =
                conversation is null
                    ? Array.Empty<ChatTurn>()
                    : await _conversations.GetHistoryAsync(
                        conversation.Id,
                        MaxHistoryMessages,
                        cancellationToken
                    );

            var stopwatch = Stopwatch.StartNew();

            var completion = await _chatService.AskAsync(
                message,
                request.Model,
                history,
                cancellationToken
            );

            stopwatch.Stop();

            conversation ??= await _conversations.CreateAsync(
                BuildTitle(message),
                completion.Model,
                cancellationToken
            );

            await _conversations.AppendExchangeAsync(
                conversation.Id,
                message,
                completion.Answer,
                completion.Model,
                stopwatch.ElapsedMilliseconds,
                cancellationToken
            );

            return Ok(
                new ChatResponse(completion.Answer, completion.Model, conversation.Id)
            );
        }
        catch (OllamaUnavailableException ex)
        {
            _logger.LogWarning(ex, "Ollama is unavailable.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { error = ex.Message }
            );
        }
        catch (OllamaTimeoutException ex)
        {
            _logger.LogWarning(ex, "Ollama timed out.");
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = ex.Message });
        }
        catch (OllamaResponseException ex)
        {
            _logger.LogWarning(ex, "Ollama returned an invalid response.");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }

    private static string BuildTitle(string message)
    {
        var normalized = Regex.Replace(message, @"\s+", " ").Trim();

        return normalized.Length <= 60 ? normalized : $"{normalized[..57]}...";
    }
}
