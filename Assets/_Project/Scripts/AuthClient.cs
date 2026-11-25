using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class AuthClient
{
    const string BaseUrl = "http://localhost:5080";

    [Serializable] class AuthRequest
    {
        public string username;
        public string password;
    }

    [Serializable] class RegisterResponse
    {
        public int id;
        public string username;
    }

    [Serializable] class LoginResponse
    {
        public string token;
        public string username;
    }

    [Serializable] class ErrorResponse
    {
        public string title;
    }

    public static IEnumerator Register(string username, string password, Action<bool, string> done)
    {
        string json = JsonUtility.ToJson(new AuthRequest { username = username, password = password });
        return Post("/api/auth/register", json, (ok, text) =>
        {
            if (!ok) { done(false, ShortError(text)); return; }
            var response = JsonUtility.FromJson<RegisterResponse>(text);
            done(response != null && response.id > 0, response != null ? "id=" + response.id : "bad response");
        });
    }

    public static IEnumerator Login(string username, string password, Action<bool, string> done)
    {
        string json = JsonUtility.ToJson(new AuthRequest { username = username, password = password });
        return Post("/api/auth/login", json, (ok, text) =>
        {
            if (!ok) { AuthSession.Clear(); done(false, ShortError(text)); return; }
            var response = JsonUtility.FromJson<LoginResponse>(text);
            if (response == null || string.IsNullOrEmpty(response.token)) { AuthSession.Clear(); done(false, "bad token"); return; }
            AuthSession.Save(response.token, response.username);
            done(true, "ok");
        });
    }

    static IEnumerator Post(string path, string json, Action<bool, string> done)
    {
        var request = new UnityWebRequest(BaseUrl + path, "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 10;

        yield return request.SendWebRequest();

        bool ok = request.result == UnityWebRequest.Result.Success &&
                  request.responseCode >= 200 && request.responseCode < 300;
        string text = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        if (!ok && string.IsNullOrEmpty(text)) text = request.error;
        done(ok, text);
        request.Dispose();
    }

    static string ShortError(string text)
    {
        if (string.IsNullOrEmpty(text)) return "Network error";
        try
        {
            var error = JsonUtility.FromJson<ErrorResponse>(text);
            if (error != null && !string.IsNullOrEmpty(error.title)) return error.title;
        }
        catch { /* ASP.NET validation payloads are not always ErrorResponse-shaped */ }
        return text.Length > 120 ? text.Substring(0, 120) : text;
    }
}
