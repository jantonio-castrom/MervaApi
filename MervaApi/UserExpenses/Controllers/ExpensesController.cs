using System.Security.Claims;
using MervaApi.Configuration;
using MervaApi.UserExpenses.Models;
using MervaApi.UserExpenses.Services;
using MervaApi.UserTokens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MervaApi.UserExpenses.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ExpensesController(
    IUserExpenseService userExpenseService,
    IUserTokenService userTokenService,
    IOptions<LimitsOptions> limits) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var isPremium = await userTokenService.GetIsPremiumAsync(tokenId);
        var fromDate = isPremium ? (DateOnly?)null : DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-3);
        var expenses = await userExpenseService.GetExpensesAsync(tokenId, fromDate);
        return Ok(expenses);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var deleted = await userExpenseService.SoftDeleteExpenseAsync(id, tokenId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddExpenseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Token is required.");
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        if (!await userTokenService.GetIsPremiumAsync(tokenId))
        {
            var count = await userTokenService.CountTransactionsAsync(tokenId);
            if (count >= limits.Value.FreeTransactionLimit)
                return StatusCode(403, "Transaction limit reached. Upgrade to premium for unlimited transactions.");
        }

        var expense = await userExpenseService.AddExpenseAsync(request);

        if (expense is null)
            return NotFound("Token not found.");

        return Created(string.Empty, new
        {
            expense.ExpenseId,
            expense.Name,
            expense.Amount,
            expense.Currency,
            expense.Category,
            expense.ExpenseDate,
            expense.CreatedAt
        });
    }
}
