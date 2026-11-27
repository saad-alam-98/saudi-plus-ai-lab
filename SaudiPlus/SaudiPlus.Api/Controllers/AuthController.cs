using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Application.Interfaces;
namespace SaudiPlus.Api.Controllers;
public sealed record Credentials([Required, EmailAddress, MaxLength(256)] string Email, [Required, MaxLength(1024)] string Password);
[ApiController]
[Route("api/auth")]
public sealed class AuthController(IIdentityService identity) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(Credentials request, CancellationToken cancellationToken) =>
        await identity.RegisterAsync(request.Email, request.Password, cancellationToken)
            ? StatusCode(201) : BadRequest(new { message = "Registration could not be completed." });
    [HttpPost("login")]
    public async Task<IActionResult> Login(Credentials request, CancellationToken cancellationToken)
    {
        var token = await identity.LoginAsync(request.Email, request.Password, cancellationToken);
        return token is null ? Unauthorized(new { message = "Invalid credentials." }) : Ok(token);
    }
}
