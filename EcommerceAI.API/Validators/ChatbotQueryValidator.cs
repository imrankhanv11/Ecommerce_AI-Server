using FluentValidation;
using EcommerceAI.Contracts.DTOs.Chatbot;

namespace EcommerceAI.API.Validators;

public class ChatbotQueryValidator : AbstractValidator<ChatbotQueryRequestDto>
{
    public ChatbotQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required")
            .MinimumLength(3).WithMessage("Query too short (min 3 characters)")
            .MaximumLength(300).WithMessage("Query too long (max 300 characters)")
            .Must(q => !ContainsSqlKeywords(q)).WithMessage("Invalid query content");
    }

    private static bool ContainsSqlKeywords(string input)
    {
        var banned = new[] { "DROP", "DELETE", "INSERT", "--", "/*", "xp_" };
        return banned.Any(k => input.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
