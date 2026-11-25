# WARFRAME: THE OLD WAR — Development Progress

Unity 6.3 LTS (6000.3.24f1) · **URP 2D template** · Input System · uGUI
Game type: **2D side-view action platformer** (sprites + Physics2D + orthographic camera). Map: 110 units wide.

## How to run

1. Open the project in Unity 6.3 LTS.
2. Press `Ctrl + R` in the Project window if new scripts are not listed yet.
3. Open `Assets/Scenes/MainGame.unity`.
4. Press **Play**.
5. Pick a Warframe and clear 3 waves.

## Controls

| Input | Action |
|---|---|
| A / D | Move left / right |
| Space | Jump (double jump) |
| Shift | Dash (0.16 s burst, 0.8 s cooldown) |
| Mouse | Aim (crosshair follows cursor) |
| LMB | Fire / melee slash |
| Q | Ability 1 |
| E | Ability 2 |
| Esc | Pause / resume |

Note: the game is 2D side-view. The project template is 2D, so the game was rebuilt from the initial 3D prototype to top-down 2D, then to the current side-view platformer.

## Architecture

The whole game is built at runtime from one scene object (`GameBootstrap`), so no prefab or inspector reference can break.

| File | Role |
|---|---|
| `GameBootstrap.cs` | Scene entry point, creates GameManager and boots the game |
| `GameManager.cs` | State machine, waves, spawning, victory/defeat, pause, cleanup, camera setup |
| `PlayerController.cs` | Rigidbody2D movement, dash, mouse aim, shooting, health, energy, 6 abilities |
| `CharacterStats.cs` | Data for Excalibur Prime / Mesa Prime / Wisp Prime |
| `Enemy.cs` | Grineer Lancer (ranged) and Infested Charger (melee), chase, attack, health bar, death |
| `Projectile.cs` | Physics2D circle-cast projectiles, hit filtering, splash, pierce |
| `CameraRig.cs` | Orthographic follow camera with mouse look-ahead and shake |
| `UIManager.cs` | HUD, character select, pause menu, victory/defeat panels, mouse crosshair |
| `ArenaBuilder.cs` | 110-unit-wide side-view arena: floor, raised platforms up to y=4.2, walls, lane markers, cover crates, neon skyline |
| `SpriteUtil.cs` | Runtime white square/circle sprites and sprite factory helpers |
| `Effect.cs` | Expanding rings and debris bursts |
| `Editor/SmokeTest.cs` | Batch-mode Play Mode smoke test (verification tool) |

## Status by phase

### Phase 1 — Foundation: COMPLETE
- Project inspected, folders created.
- `MainGame.unity` gameplay scene with `GameBootstrap`, added to build settings.
- `GameManager` created and wired.
- Orthographic camera configured at runtime, follow rig added.
- Basic UI created at runtime (canvas, EventSystem, HUD).

### Phase 2 — Player: COMPLETE
- Rigidbody2D movement, sprint, dash, collision with walls and enemies.
- Mouse aim with rotating weapon pivot.
- Orthographic camera follow with look-ahead.
- HP and energy with regeneration.
- Three visually distinct Warframes built from runtime sprites.
- Procedural animation rig for all frames: run cycle, body bob, movement lean, air pose, hit flash, cast glow.
- Frame-specific motion: Excalibur blade arc, Mesa tail wag and dual-gun recoil, Wisp hover with swaying cloth panels.

### Phase 3 — Combat: COMPLETE
- Weapon fire with per-character fire rate, damage, projectile speed.
- Projectile system with 2D circle-cast hits, splash and pierce.
- Damage calculation (Shatter Shield reduction included).
- Enemy health, hit flash, health bar above each enemy.
- Player energy costs and regeneration.
- 6 functional abilities (2 per Warframe), all executed in the smoke test.

### Phase 4 — Enemies and gameplay: COMPLETE
- 2 enemy types: Grineer Lancer (ranged, keeps distance, strafes) and Infested Charger (fast melee).
- Target detection, chase / keep-range steering, separation so they do not stack.
- Enemy attacks: projectiles for Lancer, contact damage for Charger.
- Enemy death with debris burst and kill counting.
- Wide combat arena: walls at both ends, 11 raised platforms (up to 8.2 units above the floor), cover crates, lane markers.
- Enemies spawn across the full width (12 to 43 units from center) and walk toward the player.
- Mission objective: clear 3 waves.

### Phase 5 — Characters and UI: COMPLETE
- Character selection screen with stats, weapon and ability names.
- Excalibur Prime: 150 HP / 150 EN, rifle, Radial Blind, Exalted Blade.
- Mesa Prime: 120 HP / 130 EN, rapid dual pistols, Peacemaker, Shatter Shield.
- Wisp Prime: 100 HP / 180 EN, heavy energy rifle, Sol Gate, Reservoirs.
- HP bar, energy bar, wave counter, kill counter, ability cooldown text.
- Mouse-following crosshair.
- Pause menu (resume, change Warframe, quit).
- Victory and defeat panels with mission summary.

### Phase 6 — Finalization: COMPLETE
- Compile check: 0 errors, 0 warnings.
- Batch-mode Play Mode smoke test on the 2D build: 0 errors (see `TEST_REPORT.md`).
- Scene added to build settings.

## Partially completed

- **Visual polish**: no audio and no skeletal animation. All motion is procedural on runtime sprites: run cycle with leg stride and body bob, air pose, movement lean, melee blade arc, gun recoil, ability cast glow, Wisp hover with swaying cloth, Mesa tail sway.
- **Enemy navigation**: direct steering, no pathfinding, so enemies slide along pillars.
- **Balance**: functional, not tuned.

## Not implemented (optional P2 scope)

- Boss with multiple phases.
- Six enemy types (2 implemented).
- Capture Point, Campaign, Base Defense, Survival modes.
- Advanced animations and VFX.
- Save/Load system.
- Settings menu.
