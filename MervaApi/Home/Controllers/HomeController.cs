using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MervaApi.Home.Models;

namespace MervaApi.Home.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class HomeController : ControllerBase
{
    [HttpGet]
    public HomeResponse Get() =>
        new("Welcome to Merva API test", DateTimeOffset.UtcNow);
}
