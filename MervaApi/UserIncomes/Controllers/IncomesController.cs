using System.Security.Claims;
using MervaApi.UserIncomes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MervaApi.UserIncomes.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class IncomesController(IUserIncomeService userIncomeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var incomes = await userIncomeService.GetIncomesAsync(tokenId);
        return Ok(incomes);
    }
}
