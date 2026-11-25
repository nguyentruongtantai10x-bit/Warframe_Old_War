# ASM1 Game API — ASP.NET Core + MySQL

Back-end theo `Hướng Dẫn ASM 1 - Game API ASP.NET Core.pdf`. Không gồm Unity client: đề yêu cầu thử nghiệm trên Swagger.

## Chạy nhanh

Double-click `start_api.bat` ở root project. Script tự kiểm tra cổng 5080, mở API trong cửa sổ riêng và mở Swagger. Đóng cửa sổ "WARFRAME Game API" để tắt server.

## Chạy bằng lệnh```powershell
& "C:\Users\Admin\.dotnet9\dotnet.exe" run --project "GameApi\GameApi.csproj" --no-launch-profile --urls "http://localhost:5080"
```

Mở: `http://localhost:5080/swagger`

API tự chạy migration lúc khởi động. MySQL local đã có database `game_asm1_demo` với 3 bảng: `accounts`, `characters`, `__efmigrationshistory`.

## Cấu hình

`appsettings.json`:
- `ConnectionStrings:Default` = `server=localhost;port=3306;database=game_asm1_demo;user=root;password=123456;`
- `Jwt:Key` = chuỗi 52 ký tự (>= 32 theo đề).

Đổi user/password nếu MySQL máy bạn khác.

## 4 API

| Method | Path | Token | Kết quả |
|---|---|---|---|
| POST | `/api/auth/register` | Không | 201 `id`, `username`; 409 nếu trùng |
| POST | `/api/auth/login` | Không | 200 `token`, `username`; 401 nếu sai |
| POST | `/api/characters` | Có | 201, level 1, hp 100 |
| PUT | `/api/characters/{id}` | Có | 200; 404 nếu nhân vật không phải của bạn |

Trong Swagger: bấm **Authorize**, dán **chỉ chuỗi token** (không gõ `Bearer`), rồi thử 2 API Characters.

## Kiểm thử

```powershell
powershell -ExecutionPolicy Bypass -File "GameApi\tests\smoke.ps1"
```

Kết quả lần chạy cuối:

```
PASS TC01 register (201)
PASS TC02 duplicate username (409)
PASS TC03 short password (400)
PASS TC04 wrong password (401)
PASS TC05 login (200)
PASS TC06 no token (401)
PASS TC07 swagger Bearer definition (200)
PASS TC08 create character (201)
PASS TC09 empty name (400)
PASS TC10 update own character (200)
PASS TC11 level out of range (400)
PASS EXTRA missing hp is rejected (400)
PASS SETUP register second user (201)
PASS TC12 cannot edit other user (404)
PASS DB01 password hashed
PASS DB02 character updated
== RESULT errors=0 ==
```

Script tạo username có timestamp nên chạy lại nhiều lần không trùng.

## Cấu trúc

```
GameApi/
  Models/       Account, Character
  Data/         AppDbContext
  DTOs/         Request/Response, dùng int? cho Level/Hp
  Services/     AuthService, CharacterService
  Controllers/  AuthController, CharactersController
  Migrations/   InitialCreate
  tests/        smoke.ps1 (TC01–TC12)
  tools/        dotnet-ef 8.0.31 (local)
```

## Ghi chú môi trường

- Máy có SDK 10/11 nhưng không có 8/9. Đã cài SDK **9.0.203** riêng tại `C:\Users\Admin\.dotnet9` để khớp bản demo của đề. Dùng đường dẫn `dotnet.exe` đó cho mọi lệnh project này.
- `dotnet-ef` 8.0.31 targets net8.0; khi chạy tool cần `DOTNET_ROLL_FORWARD=Major` hoặc runtime 8. Lệnh migration dùng mẫu:

```powershell
$env:DOTNET_ROLL_FORWARD = "Major"
$ef = "GameApi\tools\.store\dotnet-ef\8.0.31\dotnet-ef\8.0.31\tools\net8.0\any\dotnet-ef.dll"
& "C:\Users\Admin\.dotnet9\dotnet.exe" $ef database update --project "GameApi\GameApi.csproj" --startup-project "GameApi\GameApi.csproj"
```

- MySQL máy này báo version `26.10.0`, khác bản demo `8.0.41` trong PDF. DbContext khai báo `MySqlServerVersion(8,0,0)`; migration và cả 16 kiểm tra đều chạy được.
- Trước khi nộp: xóa `bin`, `obj`, `tools` theo checklist của đề (giữ `tests` và `Migrations`).
