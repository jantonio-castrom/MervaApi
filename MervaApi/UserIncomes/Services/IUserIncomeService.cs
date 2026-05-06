using MervaApi.UserIncomes.Models;

namespace MervaApi.UserIncomes.Services;

public interface IUserIncomeService
{
    Task<IReadOnlyList<IncomeResponse>> GetIncomesAsync(int tokenId);
}
