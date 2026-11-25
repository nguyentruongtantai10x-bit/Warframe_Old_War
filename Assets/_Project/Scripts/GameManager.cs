using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { CharacterSelect, Playing, Paused, Victory, Defeat }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState State = GameState.CharacterSelect;
    public UIManager UI;
    public CameraRig Rig;
    public PlayerController Player;
    public int Wave;
    public int TotalWaves = 3;
    public int Kills;

    readonly List<Enemy> spawned = new List<Enemy>();
    Coroutine mission;

    void Awake()
    {
        Instance = this;
    }

    public void Bootstrap()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        SetupCamera();
        ArenaBuilder.Build();

        UI = gameObject.AddComponent<UIManager>();
        UI.Build();

        State = GameState.CharacterSelect;
        SetCursor(false);
        UI.ShowCharacterSelect();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 8.5f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.035f, 0.06f);
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;

        Rig = cam.gameObject.GetComponent<CameraRig>();
        if (Rig == null) Rig = cam.gameObject.AddComponent<CameraRig>();
        Rig.SetTarget(null);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.escapeKey.wasPressedThisFrame)
        {
            if (State == GameState.Playing) PauseGame();
            else if (State == GameState.Paused) ResumeGame();
        }
    }

    public void StartGame(CharacterId id)
    {
        Time.timeScale = 1f;
        Cleanup();

        Player = PlayerFactory.Create(id, new Vector3(0f, ArenaBuilder.FloorY + 0.05f, 0f));
        Rig.SetTarget(Player.transform);
        Kills = 0;
        Wave = 0;
        State = GameState.Playing;
        SetCursor(true);
        UI.ShowHUD();

        if (mission != null) StopCoroutine(mission);
        mission = StartCoroutine(RunMission());
    }

    IEnumerator RunMission()
    {
        yield return new WaitForSeconds(1f);

        for (int w = 1; w <= TotalWaves; w++)
        {
            if (State != GameState.Playing) yield break;
            Wave = w;
            UI.SetWave(w, TotalWaves);
            UI.FlashMessage("WAVE " + w + " / " + TotalWaves);
            SpawnWave(w);

            yield return new WaitForSeconds(1.2f);

            while (spawned.Count > 0)
            {
                if (State != GameState.Playing) yield break;
                yield return null;
            }

            if (w < TotalWaves)
            {
                UI.FlashMessage("WAVE CLEARED");
                yield return new WaitForSeconds(2f);
            }
        }

        Victory();
    }

    void SpawnWave(int wave)
    {
        int count = 3 + wave * 2;
        for (int i = 0; i < count; i++)
        {
            bool left = (i % 2 == 0);
            float x = (left ? -1f : 1f) * Random.Range(12f, ArenaBuilder.HalfWidth - 12f);
            Vector3 pos = new Vector3(x, ArenaBuilder.FloorY + 0.6f, 0f);
            EnemyType type = (i % 3 == 2) ? EnemyType.Charger : EnemyType.Lancer;
            SpawnEnemy(type, wave, pos);
        }
    }

    public Enemy SpawnEnemy(EnemyType type, int wave, Vector3 position)
    {
        var go = new GameObject(type == EnemyType.Lancer ? "GrineerLancer" : "InfestedCharger");
        go.transform.position = position;
        var e = go.AddComponent<Enemy>();
        e.Init(type, wave);
        spawned.Add(e);
        return e;
    }

    public void OnEnemyKilled(Enemy e)
    {
        spawned.Remove(e);
        Kills++;
    }

    public void OnPlayerDeath()
    {
        if (State != GameState.Playing) return;
        State = GameState.Defeat;
        SetCursor(false);
        if (mission != null) StopCoroutine(mission);
        UI.ShowDefeat(Kills, Wave);
    }

    void Victory()
    {
        if (State != GameState.Playing) return;
        State = GameState.Victory;
        SetCursor(false);
        UI.ShowVictory(Kills);
    }

    public void PauseGame()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        SetCursor(false);
        UI.ShowPause();
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        SetCursor(true);
        UI.HidePause();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (mission != null) StopCoroutine(mission);
        mission = null;
        Cleanup();
        State = GameState.CharacterSelect;
        SetCursor(false);
        UI.ShowCharacterSelect();
    }

    void Cleanup()
    {
        if (Player != null) Destroy(Player.gameObject);
        Player = null;

        for (int i = spawned.Count - 1; i >= 0; i--)
            if (spawned[i] != null) Destroy(spawned[i].gameObject);
        spawned.Clear();

        for (int i = Projectile.All.Count - 1; i >= 0; i--)
            if (Projectile.All[i] != null) Destroy(Projectile.All[i].gameObject);
        Projectile.All.Clear();

        Enemy.All.Clear();
        Rig.SetTarget(null);
    }

    public void SetCursor(bool locked)
    {
        // Top-down view aims with the mouse, so the cursor always stays visible.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public int EnemiesLeft { get { return spawned.Count; } }
}
