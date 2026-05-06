namespace MervaApi.UserIncomes.Models;

public record IncomeResponse(
    int IncomeId,
    string Name,
    decimal Amount,
    string Currency,
    string? Category,
    DateOnly IncomeDate,
    DateTime CreatedAt
);
