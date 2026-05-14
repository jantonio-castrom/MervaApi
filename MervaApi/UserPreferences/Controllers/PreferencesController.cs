using System.Security.Claims;
using MervaApi.UserPreferences.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MervaApi.UserPreferences.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PreferencesController(IUserPreferenceService userPreferenceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tokenId = int.Parse(User.FindFirstValue("AnonymousTokenId")!);
        var defaultCurrency = await userPreferenceService.GetDefaultCurrencyAsync(tokenId);

        if (defaultCurrency is null)
            return NoContent();

        return Ok(new { defaultCurrency });
    }
}
