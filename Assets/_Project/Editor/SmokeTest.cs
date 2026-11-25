using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SmokeTest
{
    static float startTime;
    static int stage;
    static int errors;
    static float enemyHpAfterAbility;
    static Enemy projectileTarget;

    public static void Run()
    {
        Application.logMessageReceived += OnLog;
        EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity");
        var bs = UnityEngine.Object.FindFirstObjectByType<GameBootstrap>();
        Debug.Log("[SMOKE] scene_loaded=true bootstrap_found=" + (bs != null));
        startTime = (float)EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        EditorApplication.isPlaying = true;
    }

    static void OnLog(string condition, string stack, LogType type)
    {
        if (stack != null && stack.Contains("SearchDatabase")) return;
        if (type == LogType.Exception || type == LogType.Error)
        {
            errors++;
            Debug.Log("[SMOKE][ERROR] " + condition + " | " + stack);
        }
    }

    static void Call(object target, string method, params object[] args)
    {
        if (target == null) { Debug.Log("[SMOKE] call_skipped " + method); return; }
        MethodInfo m = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (m == null) { Debug.Log("[SMOKE] missing_method " + method); errors++; return; }
        m.Invoke(target, args);
    }

    static float Elapsed { get { return (float)EditorApplication.timeSinceStartup - startTime; } }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        float t = Elapsed;
        GameManager gm = GameManager.Instance;

        if (stage == 0 && t > 2f)
        {
            stage = 1;
            Debug.Log("[SMOKE] gm=" + (gm != null) + " state=" + (gm != null ? gm.State.ToString() : "null")
                      + " canvas=" + (Object.FindFirstObjectByType<Canvas>() != null)
                      + " eventSystem=" + (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                      + " shader=" + (SpriteUtil.Square != null ? "sprite_ok" : "NULL"));
            if (gm != null) gm.StartGame(CharacterId.ExcaliburPrime);
        }
        else if (stage == 1 && t > 3f && Enemy.All.Count > 0)
        {
            stage = 2;
            if (gm == null || gm.Player == null) { Debug.Log("[SMOKE] FAIL setup player=" + (gm != null && gm.Player != null) + " enemies=" + Enemy.All.Count); errors++; return; }
            PlayerController p = gm.Player;

            GameObject floor = GameObject.Find("Floor");
            float minX = 0f, maxX = 0f;
            for (int i = 0; i < Enemy.All.Count; i++)
            {
                if (Enemy.All[i] == null) continue;
                float x = Enemy.All[i].transform.position.x;
                if (i == 0 || x < minX) minX = x;
                if (i == 0 || x > maxX) maxX = x;
            }
            GameObject highPlatform = GameObject.Find("Platform4");
            Debug.Log("[SMOKE] arena floor_width=" + (floor != null ? floor.GetComponent<BoxCollider2D>().bounds.size.x : -1f)
                      + " high_platform_y=" + (highPlatform != null ? highPlatform.transform.position.y : -99f)
                      + " spawn_x=" + minX + ".." + maxX);

            // Move next to an enemy so AoE abilities connect
            Enemy target = Enemy.All[0];
            p.transform.position = target.transform.position + new Vector3(2f, 0.2f, 0f);
            float before = target.Health;

            // Excalibur is melee: primary attack must be a sword slash, not a projectile
            Vector3 slashDir = (target.transform.position - p.transform.position).normalized;
            var aimField0 = typeof(PlayerController).GetField("<AimDirection>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (aimField0 != null) aimField0.SetValue(p, slashDir);
            Call(p, "Shoot", slashDir);
            Debug.Log("[SMOKE] melee_slash enemy_hp_before=" + before + " after=" + (target != null ? target.Health : -1f) + " projectiles=" + Projectile.All.Count);

            Transform leg = p.transform.Find("Visual/Legs/LegL");
            Vector3 legBefore = leg != null ? leg.localPosition : Vector3.zero;
            Call(p, "Animate", 0.016f, 1f, true);
            Vector3 legAfter = leg != null ? leg.localPosition : Vector3.zero;
            Debug.Log("[SMOKE] anim leg_found=" + (leg != null) + " leg_moved=" + (legBefore != legAfter));

            p.TakeDamage(1f);
            Call(p, "UpdateFlash", 0.016f);
            Transform aura = p.transform.Find("Visual/Glow");
            Debug.Log("[SMOKE] hit_flash_aura_alpha=" + (aura != null ? aura.GetComponent<SpriteRenderer>().color.a : -1f));
            p.Heal(1f);

            Call(p, "Ability1");
            enemyHpAfterAbility = target != null ? target.Health : -1f;
            Debug.Log("[SMOKE] radial_blind enemy_hp_after=" + enemyHpAfterAbility + " kills=" + gm.Kills + " energy=" + p.Energy);

            Enemy target2 = Enemy.All.Count > 0 ? Enemy.All[0] : null;
            if (target2 != null)
            {
                Vector3 to2 = (target2.transform.position - p.transform.position).normalized;
                var aimField = typeof(PlayerController).GetField("<AimDirection>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                if (aimField != null) aimField.SetValue(p, to2);
                Call(p, "Ability2");
                Debug.Log("[SMOKE] exalted_blade kills=" + gm.Kills + " cd2=" + p.Ability2Left);
            }

            // Projectile damage test
            if (Enemy.All.Count > 0)
            {
                // Isolated target: other enemies can body-block a shared projectile.
                Enemy e2 = gm.SpawnEnemy(EnemyType.Lancer, 1, new Vector3(ArenaBuilder.HalfWidth - 4f, ArenaBuilder.FloorY + 0.6f, 0f));
                projectileTarget = e2;
                float hp2 = e2.Health;
                Vector3 from = e2.transform.position + new Vector3(-1.2f, 0f, 0f);
                Projectile.Spawn(from, (e2.AimPoint() - from).normalized, 60f, 20f, true, Color.white, 0.15f, 0f, false);
                Debug.Log("[SMOKE] projectile_spawned from_dist=" + (e2.transform.position - from).magnitude + " hp_before=" + hp2);
            }
            Debug.Log("[SMOKE] excalibur state=" + gm.State + " hp=" + p.Health + " enemies=" + gm.EnemiesLeft);
        }
        else if (stage == 2 && t > 6f)
        {
            stage = 3;
            if (projectileTarget != null)
                Debug.Log("[SMOKE] projectile_result enemy_hp=" + projectileTarget.Health);
            if (gm != null && gm.Player != null) gm.StartGame(CharacterId.MesaPrime);
            if (gm != null && gm.Player != null)
            {
                Call(gm.Player, "Ability1");
                Call(gm.Player, "Ability2");
                Debug.Log("[SMOKE] mesa abilities_ok cd1=" + gm.Player.Ability1Left + " shield=" + gm.Player.ShieldActive
                          + " energy=" + gm.Player.Energy + " hp=" + gm.Player.Health);
                Call(gm.Player, "Animate", 0.016f, 1f, true);
                Transform tail = gm.Player.transform.Find("Visual/TailPivot");
                Transform gun2 = gm.Player.transform.Find("Visual/GunPivot/Gun2");
                Debug.Log("[SMOKE] mesa_rig tail=" + (tail != null) + " gun2=" + (gun2 != null)
                          + " tail_rot=" + (tail != null ? tail.localEulerAngles.z : -1f));
            }
        }
        else if (stage == 3 && t > 7.5f)
        {
            stage = 4;
            if (gm != null && gm.Player != null) gm.StartGame(CharacterId.WispPrime);
            if (gm != null && gm.Player != null)
            {
                gm.Player.TakeDamage(50f);
                float hpBefore = gm.Player.Health;
                Call(gm.Player, "Ability1");
                Call(gm.Player, "Ability2");
                Debug.Log("[SMOKE] wisp heal_before=" + hpBefore + " after=" + gm.Player.Health
                          + " speedBuff=" + gm.Player.SpeedActive + " projectiles=" + Projectile.All.Count);
                Call(gm.Player, "Animate", 0.016f, 1f, true);
                Transform wispRibbon = gm.Player.transform.Find("Visual/RibbonL");
                Transform wispVisual = gm.Player.transform.Find("Visual");
                Debug.Log("[SMOKE] wisp_anim ribbon=" + (wispRibbon != null) + " hover_y="
                          + (wispVisual != null ? wispVisual.localPosition.y : -99f));
            }
        }
        else if (stage == 4 && t > 9f)
        {
            stage = 5;
            if (gm != null)
            {
                gm.TotalWaves = 1; // shorten mission so victory can be verified
                Debug.Log("[SMOKE] wave1 enemies=" + gm.EnemiesLeft + " kills=" + gm.Kills);
                for (int i = Enemy.All.Count - 1; i >= 0; i--)
                    if (Enemy.All[i] != null) Enemy.All[i].TakeDamage(99999f);
                Debug.Log("[SMOKE] cleared enemies=" + gm.EnemiesLeft + " kills=" + gm.Kills);
            }
        }
        else if (stage == 5 && t > 12.5f)
        {
            stage = 6;
            Debug.Log("[SMOKE] victory_check state=" + (gm != null ? gm.State.ToString() : "null") + " kills=" + (gm != null ? gm.Kills : -1));
            if (gm != null && gm.State != GameState.Victory) { Debug.Log("[SMOKE] FAIL victory_not_reached"); errors++; }
        }
        else if (stage == 6 && t > 13f)
        {
            stage = 7;
            if (gm != null) gm.ReturnToMenu();
            Debug.Log("[SMOKE] menu state=" + (gm != null ? gm.State.ToString() : "null") + " player_null=" + (gm == null || gm.Player == null));
            if (gm != null) gm.StartGame(CharacterId.MesaPrime);
        }
        else if (stage == 7 && t > 14.5f)
        {
            stage = 8;
            if (gm != null && gm.Player != null) gm.Player.TakeDamage(999999f);
            Debug.Log("[SMOKE] defeat state=" + (gm != null ? gm.State.ToString() : "null"));
        }
        else if (stage == 8 && t > 15.5f)
        {
            stage = 9;
            Debug.Log("[SMOKE] after_death player_null=" + (gm == null || gm.Player == null) + " state=" + (gm != null ? gm.State.ToString() : "null"));
            Debug.Log("[SMOKE] DONE errors=" + errors);
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(errors == 0 ? 0 : 1);
        }
    }
}
