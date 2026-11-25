using GameApi.DTOs;
using GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    readonly AuthService service;

    public AuthController(AuthService service)
    {
        this.service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await service.RegisterAsync(request);
        if (result == null) return Conflict("Tên đăng nhập đã tồn tại");
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await service.LoginAsync(request);
        if (result == null) return Unauthorized("Sai tên đăng nhập hoặc mật khẩu");
        return Ok(result);
    }
}
