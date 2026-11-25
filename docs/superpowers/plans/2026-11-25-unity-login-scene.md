# Unity Login Scene Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Warframe-styled login scene that authenticates the Unity game through the ASM1 ASP.NET Core API.

**Architecture:** `LoginBootstrap` builds the UI at runtime. `LoginUI` owns the screen and button callbacks. `AuthClient` runs `UnityWebRequest` coroutines and writes into `AuthSession`; a successful login loads `MainGame`. All UI is uGUI created in code, matching the existing runtime-built `UIManager`.

**Tech Stack:** Unity 6.3 LTS, URP 2D, uGUI, new Input System, UnityWebRequest, JsonUtility, C# coroutines.

**Spec:** `docs/superpowers/specs/2026-11-25-unity-login-scene-design.md`

## Global Constraints

- API base URL: `http://localhost:5080`; endpoints exactly `/api/auth/register` and `/api/auth/login`.
- JSON request fields: `username`, `password`; response fields: `id`, `username`, `token`.
- Token lifetime is 1 day and is issued by the existing API; no client-side token logic.
- Scene names exactly `Login` and `MainGame`, both in `EditorBuildSettings`; `Login` first.
- Runtime UI only: no prefab references, no inspector wiring beyond the single bootstrap GameObject.
- Guest play when `MainGame` is opened directly must remain intact.

## Review Focus

- API offline must show a readable error instead of hanging the login screen — test with a bad port in `AuthClient` smoke.
- Wrong password must not store a token — covered by `AuthClient` login failure case.
- Fields must be clickable in the Editor: EventSystem with `InputSystemUIInputModule` must exist — check in Play Mode smoke.
- Auto-login must not loop on failure — clear stale `PlayerPrefs` token when a request fails.
- Scene transition must not break the existing `GameBootstrap` — verified by opening `MainGame` after login in the smoke test.

---

### Task 1: Auth client and session

**Files:**
- Create: `Assets/_Project/Scripts/AuthSession.cs`
- Create: `Assets/_Project/Scripts/AuthClient.cs`

**Interfaces:**
- Produces: `AuthSession.Token/Username`, `AuthSession.Save()`, `AuthSession.Clear()`, `AuthClient.Register(user, pass, Action<bool,string>)`, `AuthClient.Login(user, pass, Action<bool,string>)`.

- [ ] **Step 1: Write a failing compile/smoke check** that references `AuthClient` and `AuthSession`. Expected: compile error (RED).
- [ ] **Step 2: Implement `AuthSession`** with PlayerPrefs-backed token and username.
- [ ] **Step 3: Implement `AuthClient`** with `UnityWebRequest`, `UploadHandlerRaw`, `DownloadHandlerBuffer`, JSON via `JsonUtility`, and a small `Runner : MonoBehaviour` for coroutines.
- [ ] **Step 4: Compile check.** Expected: 0 errors.

### Task 2: Login UI and scene

**Files:**
- Create: `Assets/_Project/Scripts/LoginUI.cs`
- Create: `Assets/_Project/Scripts/LoginBootstrap.cs`
- Create: `Assets/Scenes/Login.unity`, `Assets/Scenes/Login.unity.meta`
- Modify: `ProjectSettings/EditorBuildSettings.asset`

**Interfaces:**
- Consumes: `AuthClient` and `AuthSession` from Task 1.
- Produces: a scene named `Login` with one `LoginBootstrap` GameObject, first in build settings.

- [ ] **Step 1: Implement `LoginUI.Build()`** with the reference-art layout: dark full-screen panel, two angled gold side shapes, dust specks, centered crest + wordmark, username/password input fields with underline gold images, `LOGIN`, `SIGN UP`, `EXIT GAME`, status label.
- [ ] **Step 2: Wire buttons** to `AuthClient.Login` / `AuthClient.Register`; on success save session and load `MainGame`.
- [ ] **Step 3: Implement `LoginBootstrap.Awake()`** to add `LoginUI`, build it, and create the EventSystem with `InputSystemUIInputModule`.
- [ ] **Step 4: Author `Login.unity`** from the existing scene YAML, replacing the bootstrap script GUID; write `.meta` with a fixed GUID.
- [ ] **Step 5: Add `Login` first and `MainGame` second to `EditorBuildSettings`.** Expected: both enabled.

### Task 3: HUD session name

**Files:**
- Modify: `Assets/_Project/Scripts/UIManager.cs`

- [ ] **Step 1: Add a small `USER: <username>` label to the HUD**, visible only when `AuthSession.HasSession`. Guest play shows nothing.
- [ ] **Step 2: Compile check.** Expected: 0 errors.

### Task 4: Verification

**Files:**
- Create: `Assets/_Project/Editor/LoginSmokeTest.cs`

- [ ] **Step 1: Write the batch-mode smoke harness** that opens `Login`, enters Play Mode, registers a unique account, logs in, checks token/username/session, checks Canvas + EventSystem, then opens `MainGame` and checks `GameBootstrap`.
- [ ] **Step 2: Watch it fail before implementation** (missing types). Expected: compile error (RED).
- [ ] **Step 3: Run it after implementation** with the API running. Expected: `[LOGIN] DONE errors=0`.
- [ ] **Step 4: Record results in `TEST_REPORT.md`.**
