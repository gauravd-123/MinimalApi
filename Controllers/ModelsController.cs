using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinimalApi.Models;
using MinimalApi.Options;
using MinimalApi.Services;

namespace MinimalApi.Controllers;

[ApiController]
[Route("api/models")]
public sealed class ModelsController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly OllamaOptions _options;
    private readonly ILogger<ModelsController> _logger;

    public ModelsController(
        IChatService chatService,
        IOptions<OllamaOptions> options,
        ILogger<ModelsController> logger
    )
    {
        _chatService = chatService;
        _options = options.Value;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ModelsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<ModelsResponse>> Get(CancellationToken cancellationToken)
    {
        try
        {
            var models = await _chatService.ListModelsAsync(cancellationToken);
            return Ok(new ModelsResponse(models, _options.ChatModel));
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
            _logger.LogWarning(ex, "Ollama returned an invalid model list.");
            return StatusCode(StatusCodes.Status502BadGateway, new { error = ex.Message });
        }
    }
}
