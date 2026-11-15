using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MinimalApi.Models;
using MinimalApi.Options;

namespace MinimalApi.Services;

public sealed class OllamaChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaChatService> _logger;

    public OllamaChatService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaChatService> logger
    )
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatCompletion> AskAsync(
        string message,
        string? model = null,
        IReadOnlyList<ChatTurn>? history = null,
        CancellationToken cancellationToken = default
    )
    {
        var selectedModel = string.IsNullOrWhiteSpace(model)
            ? _options.ChatModel
            : model.Trim();

        var messages = new List<OllamaMessage>();

        if (history is { Count: > 0 })
        {
            messages.AddRange(
                history.Select(turn => new OllamaMessage(turn.Role, turn.Content))
            );
        }

        messages.Add(new OllamaMessage("user", message));

        var request = new OllamaChatRequest(selectedModel, messages, Stream: false);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "/api/chat",
                request,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Ollama returned {StatusCode} for model {Model}: {Body}",
                    (int)response.StatusCode,
                    selectedModel,
                    body
                );
                throw new OllamaResponseException(
                    $"Ollama returned HTTP {(int)response.StatusCode} for model '{selectedModel}'."
                );
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
                cancellationToken: cancellationToken
            );

            if (result?.Message?.Content is not { Length: > 0 } content)
            {
                throw new OllamaResponseException("Ollama returned an empty response.");
            }

            _logger.LogInformation(
                "Ollama replied with {Model} in {ElapsedMs} ms.",
                result.Model ?? selectedModel,
                stopwatch.ElapsedMilliseconds
            );

            return new ChatCompletion(content.Trim(), result.Model ?? selectedModel);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Could not reach Ollama at {BaseUrl}.",
                _httpClient.BaseAddress
            );
            throw new OllamaUnavailableException(
                "The local model is not reachable. Make sure Ollama is running.",
                ex
            );
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "The Ollama request timed out.");
            throw new OllamaTimeoutException("The local model took too long to respond.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Could not read Ollama's response.");
            throw new OllamaResponseException("Ollama returned an invalid response.", ex);
        }
    }

    public async Task<IReadOnlyList<string>> ListModelsAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var response = await _httpClient.GetAsync("/api/tags", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Ollama /api/tags returned {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body
                );
                throw new OllamaResponseException(
                    $"Ollama returned HTTP {(int)response.StatusCode}."
                );
            }

            var result = await response.Content.ReadFromJsonAsync<OllamaTagsResponse>(
                cancellationToken: cancellationToken
            );

            return result?.Models is null
                ? Array.Empty<string>()
                : result
                    .Models.Select(model => model.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Could not reach Ollama at {BaseUrl}.",
                _httpClient.BaseAddress
            );
            throw new OllamaUnavailableException(
                "The local model is not reachable. Make sure Ollama is running.",
                ex
            );
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "The Ollama model list request timed out.");
            throw new OllamaTimeoutException("Listing local models took too long.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Could not read Ollama's model list.");
            throw new OllamaResponseException("Ollama returned an invalid model list.", ex);
        }
    }
}
