using System.Security.Claims;
using MervaApi.UserExpenses.Models;
using MervaApi.UserExpenses.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MervaApi.UserExpenses.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ExpensesController(IUserExpenseService userExpenseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var expenses = await userExpenseService.GetExpensesAsync(tokenId);
        return Ok(expenses);
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
