namespace MervaApi.UserIncomes.Models;

public record AddIncomeRequest(
    string Token,
    string Name,
    decimal Amount,
    string? Currency,
    string? Category,
    DateOnly IncomeDate
);
