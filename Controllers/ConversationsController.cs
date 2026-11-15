using Microsoft.AspNetCore.Mvc;
using MinimalApi.Models;
using MinimalApi.Services;

namespace MinimalApi.Controllers;

[ApiController]
[Route("api/conversations")]
public sealed class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversations;

    public ConversationsController(IConversationService conversations)
    {
        _conversations = conversations;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ConversationSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConversationSummary>>> Get(
        CancellationToken cancellationToken
    )
    {
        return Ok(await _conversations.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConversationDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDetail>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var conversation = await _conversations.GetAsync(id, cancellationToken);

        return conversation is null
            ? NotFound(new { error = "Conversation not found." })
            : Ok(conversation);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _conversations.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound(new { error = "Conversation not found." });
    }
}
