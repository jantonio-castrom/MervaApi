using MervaApi.Encryption.Services;
using MervaApi.Security.RateLimit.Models;
using MervaApi.UserTokens.Models;
using MervaApi.UserTokens.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MervaApi.UserTokens.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TokensController(IUserTokenService userTokenService, IEncryptionService encryptionService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy.AnonymousRateLimit)]
    public async Task<IActionResult> Register([FromBody] RegisterTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Token is required.");

        var (userToken, isNew) = await userTokenService.RegisterAsync(
            request, HttpContext.Connection.RemoteIpAddress?.ToString());

        var body = new { userToken.TokenId, request.Token, userToken.CreatedAt, userToken.IsPremium };

        if (isNew)
            return CreatedAtAction(nameof(GetByToken), new { token = request.Token }, body);

        return Ok(body);
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicy.AnonymousRateLimit)]
    public async Task<IActionResult> Validate([FromBody] ValidateTokenRequest request)
    {        
        if (string.IsNullOrWhiteSpace(request.Token))
            return BadRequest("Token is required.");

        if (!await userTokenService.TokenExistsAsync(request.Token))
            return BadRequest("Token not found.");

        return Ok();
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetByToken(string token)
    {
        var result = await userTokenService.GetByTokenAsync(token);

        if (result is null)
            return NotFound();

        return Ok(new { result.Value.Token, result.Value.TokenId, result.Value.IsPremium });
    }
}
