using System.Security.Claims;
using GameApi.DTOs;
using GameApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/characters")]
[Authorize]
public class CharactersController : ControllerBase
{
    readonly CharacterService service;

    public CharactersController(CharacterService service)
    {
        this.service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCharacterRequest request)
    {
        var result = await service.CreateAsync(CurrentAccountId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCharacterRequest request)
    {
        var result = await service.UpdateAsync(CurrentAccountId, id, request);
        if (result == null) return NotFound("Không tìm thấy nhân vật của bạn");
        return Ok(result);
    }

    int CurrentAccountId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
