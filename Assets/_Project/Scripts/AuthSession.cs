using UnityEngine;

public static class AuthSession
{
    const string TokenKey = "wf_auth_token";
    const string UserKey = "wf_auth_user";

    public static string Token { get; private set; } = PlayerPrefs.GetString(TokenKey, string.Empty);
    public static string Username { get; private set; } = PlayerPrefs.GetString(UserKey, string.Empty);
    public static bool HasSession => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Username);

    public static void Save(string token, string username)
    {
        Token = token ?? string.Empty;
        Username = username ?? string.Empty;
        PlayerPrefs.SetString(TokenKey, Token);
        PlayerPrefs.SetString(UserKey, Username);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        Token = string.Empty;
        Username = string.Empty;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UserKey);
        PlayerPrefs.Save();
    }
}
