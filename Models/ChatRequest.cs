using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Models;

public sealed class ChatRequest
{
    [Required(ErrorMessage = "Message is required.")]
    [StringLength(8000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 1)]
    public string? Model { get; set; }

    public Guid? ConversationId { get; set; }
}
