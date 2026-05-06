using MervaApi.Data;
using MervaApi.Encryption.Services;
using MervaApi.Security;
using MervaApi.UserExpenses.Services;
using MervaApi.UserIncomes.Services;
using MervaApi.UserTokens.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

//Local usage
//var key = Convert.ToBase64String(
//    RandomNumberGenerator.GetBytes(32)
//);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<MervaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MervaDb")));

builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IUserTokenService, UserTokenService>();
builder.Services.AddScoped<IUserExpenseService, UserExpenseService>();
builder.Services.AddScoped<IUserIncomeService, UserIncomeService>();
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
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
