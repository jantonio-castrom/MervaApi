namespace MervaApi.UserExpenses.Models;

public record AddExpenseRequest(
    string Token,
    string Name,
    decimal Amount,
    string? Currency,
    string? Category,
    DateOnly ExpenseDate
);
