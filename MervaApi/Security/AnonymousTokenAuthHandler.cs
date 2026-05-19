using System.Security.Claims;
using System.Text.Encodings.Web;
using MervaApi.Data;
using MervaApi.Encryption.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace MervaApi.Security
{   
    public class AnonymousTokenAuthHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly MervaDbContext _dbContext;
        private readonly IEncryptionService _encryptionService;
        public AnonymousTokenAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,            
            MervaDbContext dbContext,
            IEncryptionService encryptionService)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
            _encryptionService = encryptionService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Check Authorization header
                if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    return AuthenticateResult.Fail("Missing Authorization Header");
                }

                var authorization = authHeader.ToString();

                // Validate Bearer format
                if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticateResult.Fail("Invalid Authorization Format");
                }

                // Extract raw token
                var rawToken = authorization["Bearer ".Length..].Trim();

                if (string.IsNullOrWhiteSpace(rawToken))
                {
                    return AuthenticateResult.Fail("Missing Token");
                }                
                // Hash token
                var tokenHash = _encryptionService.ComputeSha256(rawToken);
                // Lookup token in DB
                var tokenEntity = await _dbContext.UserTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.EncryptedValueHash == tokenHash);

                if (tokenEntity is null)
                {
                    return AuthenticateResult.Fail("Invalid Token");
                }

                // Create claims
                var claims = new[]
                {
                    new Claim("AnonymousTokenId", tokenEntity.TokenId.ToString()),
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);

                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Authentication error");
                return AuthenticateResult.Fail("Authentication Error");
            }
        }     
    }
}
