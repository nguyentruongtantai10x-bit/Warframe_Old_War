# Git upload checklist

Repo: `D:\DLhoc\unity\Warframe_Old_War`, branch `main`, 0 tracked files before this check.

## Ready to add

- `Assets/Scenes/Login.unity`, `MainGame.unity` + `.meta`.
- `Assets/_Project/Scripts/*.cs` + `.meta` (game + login client).
- `Assets/_Project/Editor/SmokeTest.cs`, `LoginSmokeTest.cs` + `.meta`.
- `Assets/Settings`, `Packages`, `ProjectSettings` (build settings list Login then MainGame).
- `GameApi/` source, `Migrations/`, `appsettings.json`, `tests/smoke.ps1`, `README.md`.
- `start_api.bat` (launcher 1-click, nằm ở root).
- `TEST_REPORT.md`, `DEVELOPMENT_PROGRESS.md`, `GIT_UPLOAD_CHECKLIST.md`, `docs/superpowers/...`.

## Ignored by `.gitignore`

- Unity: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, auto-generated `*.csproj`, `*.slnx`, `ignore.conf`, `.plastic/`.
- API: `GameApi/bin/`, `GameApi/obj/`, `GameApi/tools/`, `GameApi/.sdd/`.
- Local scratch: `.superpowers/`, `*.log`.

## Verification done

- Unity compile: 0 errors.
- API smoke: 16 checks, `errors=0`.
- Login smoke: register → login → token → UI → build settings, `errors=0`.
- Scene `.meta` GUIDs unique; build settings GUIDs valid.

## Before public push

`GameApi/appsettings.json` contains local MySQL `root/123456` and a demo JWT key. The PDF allows local ASM secrets, but a public GitHub repo exposes them. Either keep the repo private, or replace both with placeholders and supply real values locally before pushing.

## Commands

```bash
git add .
git status --short
git commit -m "feat: ASM1 game API and Unity login scene"
```
