namespace MervaApi.UserExpenses.Models;

public record ExpenseResponse(
    int ExpenseId,
    string Name,
    decimal Amount,
    string Currency,
    string? Category,
    DateOnly ExpenseDate,
    DateTime CreatedAt
);
