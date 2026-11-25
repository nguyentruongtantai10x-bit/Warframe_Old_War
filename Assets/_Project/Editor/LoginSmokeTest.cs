using System;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class LoginSmokeTest
{
    static float startTime;
    static int stage;
    static int errors;

    class Runner : MonoBehaviour { }

    public static void Run()
    {
        Application.logMessageReceived += OnLog;
        EditorSceneManager.OpenScene("Assets/Scenes/Login.unity");
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
            Debug.Log("[LOGIN][ERROR] " + condition + " | " + stack);
        }
    }

    static void Fail(string message)
    {
        errors++;
        Debug.Log("[LOGIN] FAIL " + message);
    }

    static IEnumerator Flow()
    {
        string user = "smoke_" + DateTime.Now.Ticks;
        string pass = "123456";
        bool registerOk = false;
        string registerMessage = "";
        yield return AuthClient.Register(user, pass, (ok, message) => { registerOk = ok; registerMessage = message; });
        Debug.Log("[LOGIN] register ok=" + registerOk + " message=" + registerMessage);
        if (!registerOk) Fail("register_failed");

        bool loginOk = false;
        string loginMessage = "";
        yield return AuthClient.Login(user, pass, (ok, message) => { loginOk = ok; loginMessage = message; });
        Debug.Log("[LOGIN] login ok=" + loginOk + " token_len=" + AuthSession.Token.Length
                  + " username=" + AuthSession.Username + " has_session=" + AuthSession.HasSession);
        if (!loginOk || AuthSession.Token.Length < 20 || AuthSession.Username != user) Fail("login_failed");

        bool canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>() != null;
        bool eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null;
        bool inputModule = UnityEngine.Object.FindFirstObjectByType<InputSystemUIInputModule>() != null;
        Debug.Log("[LOGIN] ui canvas=" + canvas + " eventSystem=" + eventSystem + " inputModule=" + inputModule);
        if (!canvas || !eventSystem || !inputModule) Fail("ui_missing");

        var ui = UnityEngine.Object.FindFirstObjectByType<LoginUI>();
        var modeField = typeof(LoginUI).GetField("registerMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var onResponse = typeof(LoginUI).GetMethod("OnResponse", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (ui != null && modeField != null && onResponse != null)
        {
            modeField.SetValue(ui, true);
            onResponse.Invoke(ui, new object[] { true, "ok" });
            bool stillRegister = (bool)modeField.GetValue(ui);
            Debug.Log("[LOGIN] register_mode_reset=" + (!stillRegister));
            if (stillRegister) Fail("register_mode_not_reset");
        }
        else Fail("ui_reflection_missing");

        bool mainSceneEnabled = false;
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled && s.path == "Assets/Scenes/MainGame.unity") mainSceneEnabled = true;
        Debug.Log("[LOGIN] main_scene_in_build=" + mainSceneEnabled + " bootstrap_type=" + (typeof(GameBootstrap) != null));
        if (!mainSceneEnabled) Fail("main_scene_missing");

        Debug.Log("[LOGIN] DONE errors=" + errors);
        EditorApplication.isPlaying = false;
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        if (stage == 0 && (float)EditorApplication.timeSinceStartup - startTime > 2f)
        {
            stage = 1;
            Debug.Log("[LOGIN] scene_loaded=" + SceneManager.GetActiveScene().name);
            var go = new GameObject("LoginSmokeRunner");
            go.AddComponent<Runner>().StartCoroutine(Flow());
        }
    }
}
