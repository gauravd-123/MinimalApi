using MinimalApi.Models;

namespace MinimalApi.Services;

public interface IChatService
{
    Task<ChatCompletion> AskAsync(
        string message,
        string? model = null,
        IReadOnlyList<ChatTurn>? history = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<string>> ListModelsAsync(CancellationToken cancellationToken = default);
}
