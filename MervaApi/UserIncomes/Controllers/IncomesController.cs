using System.Security.Claims;
using MervaApi.UserIncomes.Models;
using MervaApi.UserIncomes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MervaApi.UserIncomes.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class IncomesController(IUserIncomeService userIncomeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddIncomeRequest request)
    {
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
