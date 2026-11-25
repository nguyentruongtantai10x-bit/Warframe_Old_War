using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    Font font;
    InputField userField;
    InputField passField;
    Text status;
    Text actionText;
    bool registerMode;

    readonly Color gold = new Color(0.78f, 0.66f, 0.34f);
    readonly Color pale = new Color(0.92f, 0.90f, 0.80f);
    readonly Color dim = new Color(0.55f, 0.52f, 0.42f);

    public void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        var canvasGo = new GameObject("LoginCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform root = canvasGo.transform;
        Rect(root, "Backdrop", Vector2.zero, new Vector2(2400f, 1400f), new Color(0.025f, 0.026f, 0.022f));

        // Angled side panels and thin gold seams from the reference art.
        Rect(root, "PanelL", new Vector2(-700f, 0f), new Vector2(340f, 1700f), new Color(0.07f, 0.065f, 0.04f), 12f);
        Rect(root, "PanelR", new Vector2(700f, 0f), new Vector2(340f, 1700f), new Color(0.07f, 0.065f, 0.04f), -12f);
        Rect(root, "SeamL", new Vector2(-520f, 0f), new Vector2(3f, 1700f), new Color(gold.r, gold.g, gold.b, 0.55f), 12f);
        Rect(root, "SeamR", new Vector2(520f, 0f), new Vector2(3f, 1700f), new Color(gold.r, gold.g, gold.b, 0.55f), -12f);

        Random.InitState(42);
        for (int i = 0; i < 90; i++)
        {
            float x = Random.Range(-900f, 900f);
            float y = Random.Range(-520f, 520f);
            float s = Random.Range(1f, 3.5f);
            Rect(root, "Dust" + i, new Vector2(x, y), new Vector2(s, s), new Color(gold.r, gold.g, gold.b, Random.Range(0.12f, 0.45f)));
        }

        // Crest: stacked diamonds.
        for (int i = 0; i < 3; i++)
        {
            float s = 58f - i * 16f;
            Rect(root, "Crest" + i, new Vector2(0f, 250f - i * 4f), new Vector2(s, s),
                i == 0 ? new Color(gold.r, gold.g, gold.b, 0.35f) : gold, 45f);
        }

        Label(root, "Wordmark", "WARFRAME", new Vector2(0f, 128f), new Vector2(900f, 100f), 82, pale, TextAnchor.MiddleCenter);
        Label(root, "Version", "U I   U P D A T E   |   L O G I N", new Vector2(0f, 70f), new Vector2(700f, 40f), 18, dim, TextAnchor.MiddleCenter);

        userField = Field(root, "Username", "EMAIL ADDRESS", new Vector2(0f, -30f), false);
        passField = Field(root, "Password", "PASSWORD", new Vector2(0f, -130f), true);

        MakeButton(root, "LOGIN", new Vector2(0f, -232f), new Vector2(260f, 62f), () => Submit());

        status = Label(root, "Status", "", new Vector2(0f, -302f), new Vector2(900f, 44f), 20, gold, TextAnchor.MiddleCenter);

        MakeButton(root, "SIGN UP", new Vector2(-820f, -480f), new Vector2(220f, 54f), () => ToggleMode(), dim);
        MakeButton(root, "EXIT GAME", new Vector2(820f, -480f), new Vector2(220f, 54f), () => Application.Quit(), dim);

        if (AuthSession.HasSession)
        {
            status.text = "SESSION ACTIVE  //  " + AuthSession.Username;
            actionText.text = "CONTINUE";
        }
    }

    Image Rect(Transform parent, string name, Vector2 pos, Vector2 size, Color color, float angle = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    Text Label(Transform parent, string name, string value, Vector2 pos, Vector2 size, int sizePt, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = sizePt;
        text.color = color;
        text.alignment = anchor;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    InputField Field(Transform parent, string name, string placeholder, Vector2 pos, bool password)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(560f, 60f);
        go.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.05f, 0.7f);

        Rect(go.transform, "Underline", new Vector2(0f, -32f), new Vector2(560f, 2f), new Color(gold.r, gold.g, gold.b, 0.8f));

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 6f);
        textRect.offsetMax = new Vector2(-12f, -6f);
        var text = textGo.GetComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.color = pale;
        text.supportRichText = false;

        var hint = Label(go.transform, "Placeholder", placeholder, Vector2.zero, new Vector2(536f, 48f), 22, dim, TextAnchor.MiddleLeft);
        hint.transform.localPosition = new Vector3(0f, 0f, 0f);

        var field = go.GetComponent<InputField>();
        field.textComponent = text;
        field.placeholder = hint;
        field.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
        field.characterLimit = 50;
        return field;
    }

    void MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, Color? color = null)
    {
        var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;
        go.GetComponent<Image>().color = new Color(0.08f, 0.075f, 0.05f, 0.85f);
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(action);
        var text = Label(go.transform, "Label", label, Vector2.zero, size, 24, color ?? gold, TextAnchor.MiddleCenter);
        if (label == "LOGIN") actionText = text;
    }

    void ToggleMode()
    {
        registerMode = !registerMode;
        actionText.text = registerMode ? "REGISTER" : "LOGIN";
        status.text = registerMode ? "CREATE ACCOUNT  //  MIN 6 CHARACTER PASSWORD" : "";
    }

    void Submit()
    {
        string user = userField != null ? userField.text.Trim() : string.Empty;
        string pass = passField != null ? passField.text : string.Empty;
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            status.text = "ENTER USERNAME AND PASSWORD";
            return;
        }

        status.text = "CONNECTING...";
        status.color = dim;
        System.Collections.IEnumerator request = registerMode
            ? AuthClient.Register(user, pass, OnResponse)
            : AuthClient.Login(user, pass, OnResponse);
        StartCoroutine(request);
    }

    void OnResponse(bool ok, string message)
    {
        status.color = ok ? gold : new Color(0.95f, 0.45f, 0.35f);
        if (!ok)
        {
            status.text = "ERROR  //  " + message;
            return;
        }
        if (registerMode)
        {
            registerMode = false;
            if (actionText != null) actionText.text = "LOGIN";
            status.text = "ACCOUNT CREATED  //  PLEASE LOGIN";
        }
        else
        {
            status.text = "AUTHORIZED  //  LOADING GAME";
            StartCoroutine(LoadGame());
        }
    }

    System.Collections.IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(0.4f);
        SceneManager.LoadScene("MainGame");
    }
}
