using GameApi.Data;
using GameApi.DTOs;
using GameApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GameApi.Services;

public class CharacterService
{
    readonly AppDbContext db;

    public CharacterService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<CharacterResponse> CreateAsync(int accountId, CreateCharacterRequest request)
    {
        var character = new Character
        {
            Name = request.Name,
            Class = request.Class,
            AccountId = accountId,
            Level = 1,
            Hp = 100,
            CreatedAt = DateTime.UtcNow
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return ToResponse(character);
    }

    public async Task<CharacterResponse?> UpdateAsync(int accountId, int id, UpdateCharacterRequest request)
    {
        var character = await db.Characters.FirstOrDefaultAsync(x => x.Id == id && x.AccountId == accountId);
        if (character == null) return null;

        character.Name = request.Name;
        character.Class = request.Class;
        character.Level = request.Level!.Value;
        character.Hp = request.Hp!.Value;
        await db.SaveChangesAsync();
        return ToResponse(character);
    }

    static CharacterResponse ToResponse(Character c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Class = c.Class,
        Level = c.Level,
        Hp = c.Hp,
        AccountId = c.AccountId
    };
}
