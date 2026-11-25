# ASM1 Game API — Short Design

## Intent

Build the back-end required by `Hướng Dẫn ASM 1 - Game API ASP.NET Core.pdf`: register, login with JWT, create and edit characters. Test on Swagger. No Unity client: the PDF explicitly says game UI is not part of ASM1.

## Goals

- Four endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/characters`, `PUT /api/characters/{id}`.
- Five horizontal layers: Models, Data, DTOs, Services, Controllers. Controllers never touch `AppDbContext`.
- EF Core migrations create `game_asm1_demo` with `accounts`, `characters`, `__efmigrationshistory`.
- Password hashed with `PasswordHasher`; `PasswordHash` never leaves the API.
- Account id always read from JWT claim `NameIdentifier`; a user cannot edit another user's character (404).
- Swagger with `Authorize` (Bearer JWT).
- 12 test cases TC01–TC12 pass.

## Non-goals

- Unity client integration, roles, refresh token, logout, email, save/load.

## Constraints

- PDF demo stack: .NET SDK 9.0.203, EF Core 8.0.31, Pomelo 8.0.3, JwtBearer 8.0.31, Swashbuckle 6.9.0, MySQL 8.0.41.
- Local machine has SDK 10/11 only; install SDK 9.0.203 side-by-side under `C:\Users\Admin\.dotnet9`.
- Local MySQL server runs on 3306 with `root/123456`, server version `26.10.0`.
- All EF packages must share major 8.

## Approaches

1. **Install .NET 9 SDK, use EF/Pomelo 8 (chosen).** Matches the PDF and the instructor's demo exactly. Costs one SDK download; no conflict with SDK 10/11.
2. Use installed SDK 10 with EF/Pomelo 10. Runs immediately but violates the "same major 8/9" instruction.
3. Target `net9.0` with SDK 10 plus roll-forward. No download, but runtime 10 must host a net9 app.

## Decisions

- Project at `GameApi/`, beside `Assets`, so Unity does not import back-end files.
- Connection string in `GameApi/appsettings.json`; JWT key also there (PDF allows local ASM secrets).
- `Database.Migrate()` at startup, plus explicit `dotnet ef database update` during setup.
- Integration test script `GameApi/tests/smoke.ps1` drives real HTTP calls; 12 cases asserted by status code and body.
- If Pomelo 8 cannot talk to MySQL 26.10, record the failure and switch the EF server version declaration only; do not change package majors.
