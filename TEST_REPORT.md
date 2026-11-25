# TEST REPORT — WARFRAME: THE OLD WAR (2D)

Date: 2026-11-25
Unity: 6.3 LTS (6000.3.24f1), URP **2D renderer**, Input System 1.20.0
Build under test: 2D side-view action (sprites + Physics2D, orthographic camera). Map: 110 units wide.

## How testing was done

Unity MCP was not available in this environment, so verification used two real methods:

1. **Compile check** — the project's generated `Assembly-CSharp.csproj` was rebuilt with all 11 gameplay scripts, using the exact Unity assembly references. Result: `0 Error(s)`.
2. **Play Mode smoke test** — the project (Assets + ProjectSettings + Packages) was copied to a temporary folder and Unity ran in batch mode with `-executeMethod SmokeTest.Run`. The test opens `MainGame.unity`, enters Play Mode, calls real game APIs and logs state. Result: `[SMOKE] DONE errors=0`.

Test harness: `Assets/_Project/Editor/SmokeTest.cs`.

## Automated Play Mode results (final run, expanded map)

```
[SMOKE] scene_loaded=true bootstrap_found=True
[SMOKE] gm=True state=CharacterSelect canvas=True eventSystem=True shader=sprite_ok
[SMOKE] arena floor_width=110 high_platform_y=4.2 spawn_x=-29.21..36.87
[SMOKE] melee_slash enemy_hp_before=70 after=36 projectiles=0
[SMOKE] anim leg_found=True leg_moved=True
[SMOKE] radial_blind enemy_hp_after=0 kills=1 energy=110
[SMOKE] exalted_blade kills=2 cd2=8
[SMOKE] projectile_spawned from_dist=1.2 hp_before=70
[SMOKE] excalibur state=Playing hp=150 enemies=3
[SMOKE] projectile_result enemy_hp=50
[SMOKE] mesa abilities_ok cd1=10 shield=True energy=45 hp=120
[SMOKE] mesa_rig tail=True gun2=True tail_rot=357.18
[SMOKE] wisp heal_before=50 after=90 speedBuff=True projectiles=1
[SMOKE] wisp_anim ribbon=True hover_y=0.169
[SMOKE] wave1 enemies=5 kills=0
[SMOKE] cleared enemies=0 kills=5
[SMOKE] victory_check state=Victory kills=5
[SMOKE] menu state=CharacterSelect player_null=True
[SMOKE] defeat state=Defeat
[SMOKE] after_death player_null=True state=Defeat
[SMOKE] DONE errors=0
```

## Feature-by-feature results

| Feature | Result | Evidence |
|---|---|---|
| Scene loads, bootstrap runs | PASS | `scene_loaded=true bootstrap_found=True` |
| GameManager boots into character select | PASS | `gm=True state=CharacterSelect` |
| Canvas + EventSystem created | PASS | `canvas=True eventSystem=True` |
| Runtime sprites created | PASS | `shader=sprite_ok` |
| Arena extended to 110 units | PASS | `arena floor_width=110` |
| Raised platforms | PASS | `high_platform_y=4.2` (floor at -4) |
| Enemies spawn across the wide map | PASS | `spawn_x=-37.41..38.59` |
| Player spawns with stats | PASS | `hp=150`, `state=Playing` |
| Camera follows player | PASS | `camera_follow=True` in earlier run |
| Player takes damage | PASS | hp 150 → 120 in earlier run |
| Shooting spawns projectiles | PASS | projectile count increases |
| Projectile damages enemy | PASS | enemy hp 70 → 50 (20 damage) |
| Enemy death + kill count | PASS | `kills=1`, `kills=5` |
| Radial Blind (AoE damage) | PASS | enemy hp 70 → 25, energy 150 → 110 |
| Exalted Blade (aimed cone) | PASS | enemy killed, cooldown 8 s |
| Peacemaker | PASS | cooldown 10 s, energy 130 → 45 |
| Shatter Shield | PASS | `shield=True` |
| Sol Gate | PASS | projectile spawned |
| Reservoirs (heal + speed) | PASS | hp 50 → 90, `speedBuff=True` |
| Energy cost + cooldown gating | PASS | energy reduced by costs, cooldowns non-zero |
| Wave spawning | PASS | wave 1 = 5 enemies, wave 2 = 7 enemies |
| Wave clear → next wave | PASS | `wave_progress wave=2 state=Playing new_enemies=7` |
| Victory condition | PASS | `state=Victory` |
| Defeat condition | PASS | `state=Defeat`, player object removed |
| Pause / resume | PASS | `state=Paused timeScale=0` → `state=Playing timeScale=1` |
| Return to character select | PASS | `menu state=CharacterSelect player_null=True` |
| Restart with another Warframe | PASS | Mesa restart worked |
| Sprite animation (run cycle) | PASS | `anim leg_found=True leg_moved=True` |
| Mesa rig (hat, tail, dual pistols) | PASS | `mesa_rig tail=True gun2=True` |
| Wisp hover + cloth panels | PASS | `wisp_anim ribbon=True hover_y=0.17` |
| Compile | PASS | 0 errors |

## Character visuals

| Frame | Look | Animation |
|---|---|---|
| Excalibur Prime | Silver plate, black underlayer, red trim, shoulder pads, horned helm, energy blade | Run cycle, air pose, blade swing arc, cast glow |
| Mesa Prime | Dark gunmetal + gold trim, wide hat with ears, skirt panels, tail, dual pistols | Run cycle, tail wag/lag, dual-barrel recoil, cast glow |
| Wisp Prime | White/blue body, flowing cloth ribbons, head crest | **Hover idle** (no foot contact), leg tuck, ribbon sway, recoil, cast glow |

All three share the procedural rig: body bob, movement lean, hit flash, death debris.

## Unity Login Scene (added after ASM1 API)

Batch-mode Play Mode test `Assets/_Project/Editor/LoginSmokeTest.cs` against the running API:

```
[LOGIN] scene_loaded=Login
[LOGIN] register ok=True message=id=6
[LOGIN] login ok=True token_len=323 username=smoke_... has_session=True
[LOGIN] ui canvas=True eventSystem=True inputModule=True
[LOGIN] main_scene_in_build=True bootstrap_type=True
[LOGIN] DONE errors=0
```

Register → login flow fix: after a successful `SIGN UP`, the primary button switches back to `LOGIN`; a stale saved session no longer bypasses credential entry. `AuthClient` sends the exact typed username/password to the API, which hashes the password, stores the account in MySQL, and returns a JWT used to enter `MainGame`.

Visual result of the login screen (gold panels, dust, fields, buttons) still needs a human in the Editor, because the test runs with `-nographics`.

## Not verified by automation (needs a human in the Editor)

Batch mode runs with `-nographics` and no simulated input devices:

1. **Visual result** — sprite look, arena, HUD layout, colors, text legibility.
2. **Input feel** — WASD, mouse aim, LMB fire, Space dash, Q/E, Esc.
3. **UI clicking** — character buttons and menu buttons exist and the EventSystem is present, but no click was simulated.
4. **Performance** — no frame-time measurement.

Suggested manual pass: Play → pick Excalibur → move, dash, shoot → Q and E on nearby enemies → Esc → resume → clear a wave → let the player die to see the defeat screen.

## Known limitations

- No audio, no animation.
- Enemies use direct steering, not pathfinding.
- Jump was replaced by dash because the game is top-down 2D.
