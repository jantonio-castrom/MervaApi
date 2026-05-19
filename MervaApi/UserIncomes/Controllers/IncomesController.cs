using System.Security.Claims;
using MervaApi.Configuration;
using MervaApi.UserIncomes.Models;
using MervaApi.UserIncomes.Services;
using MervaApi.UserTokens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MervaApi.UserIncomes.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class IncomesController(
    IUserIncomeService userIncomeService,
    IUserTokenService userTokenService,
    IOptions<LimitsOptions> limits) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddIncomeRequest request)
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        if (!await userTokenService.GetIsPremiumAsync(tokenId))
        {
            var count = await userTokenService.CountTransactionsAsync(tokenId);
            if (count >= limits.Value.FreeTransactionLimit)
                return StatusCode(403, "Transaction limit reached. Upgrade to premium for unlimited transactions.");
        }

        var income = await userIncomeService.AddIncomeAsync(request);
        if (income is null)
            return Unauthorized();
        return CreatedAtAction(nameof(GetAll), new { }, income.IncomeId);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var incomes = await userIncomeService.GetIncomesAsync(tokenId);
        return Ok(incomes);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var deleted = await userIncomeService.SoftDeleteIncomeAsync(id, tokenId);
        return deleted ? NoContent() : NotFound();
    }
}
