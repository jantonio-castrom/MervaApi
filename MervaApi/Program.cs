using MervaApi.Configuration;
using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.Health;
using MervaApi.Security;
using MervaApi.Security.RateLimit.Models;
using MervaApi.UserExpenses.Services;
using MervaApi.UserIncomes.Services;
using MervaApi.UserPreferences.Services;
using MervaApi.UserTokens.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//Local usage
//var key = Convert.ToBase64String(
//    RandomNumberGenerator.GetBytes(32)
//);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://mervane.com",
                "https://www.mervane.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter(RateLimitPolicy.AnonymousRateLimit, limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;              // 5 attempts
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;               // reject immediately
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddDbContext<MervaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MervaDb")));

builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IUserTokenService, UserTokenService>();
builder.Services.AddScoped<IUserExpenseService, UserExpenseService>();
builder.Services.AddScoped<IUserIncomeService, UserIncomeService>();
builder.Services.AddScoped<IUserPreferenceService, UserPreferenceService>();
builder.Services.Configure<LimitsOptions>(builder.Configuration.GetSection(LimitsOptions.Section));
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");
builder.Services.AddControllers();
builder.Services.AddAuthentication("AnonymousToken")
    .AddScheme<AuthenticationSchemeOptions, AnonymousTokenAuthHandler>(
        "AnonymousToken",
        null);

builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else {
    app.UseCors("Production");
}
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.Run();
