using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameApi.Data;
using GameApi.DTOs;
using GameApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GameApi.Services;

public class AuthService
{
    readonly AppDbContext db;
    readonly IConfiguration config;
    readonly PasswordHasher<Account> hasher = new();

    public AuthService(AppDbContext db, IConfiguration config)
    {
        this.db = db;
        this.config = config;
    }

    public async Task<AccountResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await db.Accounts.AnyAsync(x => x.Username == request.Username)) return null;

        var account = new Account { Username = request.Username, CreatedAt = DateTime.UtcNow };
        account.PasswordHash = hasher.HashPassword(account, request.Password);
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return new AccountResponse { Id = account.Id, Username = account.Username };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Username == request.Username);
        if (account == null) return null;

        var result = hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        return new LoginResponse { Token = CreateToken(account), Username = account.Username };
    }

    string CreateToken(Account account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        Claim[] claims =
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(ClaimTypes.Name, account.Username)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
