using MervaApi.UserExpenses.Models;

namespace MervaApi.UserExpenses.Services;

public interface IUserExpenseService
{
    Task<Expense?> AddExpenseAsync(AddExpenseRequest request);
    Task<IReadOnlyList<ExpenseResponse>> GetExpensesAsync(int tokenId);
}
