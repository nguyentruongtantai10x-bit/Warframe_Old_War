# Unity Login Scene — Short Design

## Intent

Add a real login screen in front of the game so a player authenticates against the ASM1 Game API (and therefore MySQL) before choosing a Warframe. Visual direction: the supplied Warframe login reference — near-black field, thin gold angled side panels, sparse dust particles, centered gold crest + `WARFRAME` wordmark, two underlined fields, `LOGIN`, `SIGN UP`, `EXIT GAME`.

## Goals

- New `Assets/Scenes/Login.unity`, first in build settings; `MainGame` remains second and still playable directly for development.
- Runtime-built uGUI (same pattern as `UIManager`): no prefab or inspector wiring.
- `AuthClient`: `UnityWebRequest` POST to `/api/auth/register` and `/api/auth/login` on `http://localhost:5080`; `JsonUtility` parses `id`, `username`, `token`.
- `AuthSession` keeps token + username for the session; `PlayerPrefs` enables auto-login on the next launch.
- Success loads `MainGame`; failure shows an on-screen status message.
- Optional HUD line shows the logged-in username.

## Non-goals

- No direct MySQL connection from Unity (no credentials in client, no native DLL).
- No token refresh, logout, password recovery, or server changes.

## Constraints

- Unity 6.3 LTS, URP 2D template, new Input System, uGUI, runtime-built UI.
- API must be running at `http://localhost:5080`; base URL in one constant.
- Scene file authored as YAML like the existing `MainGame.unity`, with matching `.meta` GUIDs.

## Approaches

1. **Separate Login scene (chosen).** Matches the request ("1 scene đăng nhập"), keeps game bootstrap untouched, but adds a scene and a build-settings entry.
2. Login panel inside `MainGame`. Fewer files, but not a separate scene.
3. Unity connects directly to MySQL. Rejected: leaks credentials, needs a native connector, and bypasses the API built for ASM1.

## Decisions

- Reference art is recreated procedurally with uGUI rectangles/text; no image asset file was attached to the workspace.
- Guest play is still possible by opening `MainGame` directly, so existing smoke tests and development do not break.
- Verification: compile check plus a batch-mode Play Mode test that drives `AuthClient.Register` and `AuthClient.Login` against the running API.
