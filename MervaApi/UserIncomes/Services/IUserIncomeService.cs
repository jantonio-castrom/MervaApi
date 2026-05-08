using MervaApi.UserIncomes.Models;

namespace MervaApi.UserIncomes.Services;

public interface IUserIncomeService
{
    Task<UserIncome?> AddIncomeAsync(AddIncomeRequest request);
    Task<IReadOnlyList<IncomeResponse>> GetIncomesAsync(int tokenId);
    Task<bool> SoftDeleteIncomeAsync(int incomeId, int tokenId);
}
