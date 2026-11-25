using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Font font;
    Canvas canvas;

    GameObject hud;
    GameObject selectPanel;
    GameObject pausePanel;
    GameObject endPanel;

    Text charNameText;
    Text userText;
    Text waveText;
    Text killsText;
    Text hpText;
    Text energyText;
    Text ability1Text;
    Text ability2Text;
    Text messageText;
    Text endTitle;
    Text endBody;

    RectTransform hpFill;
    RectTransform energyFill;
    RectTransform cross1;
    RectTransform cross2;
    const float BarWidth = 340f;

    float messageTimer;

    // ---------------------------------------------------------------- build

    public void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        EnsureEventSystem();
        BuildCanvas();
        BuildHUD();
        BuildSelectPanel();
        BuildPausePanel();
        BuildEndPanel();

        hud.SetActive(false);
        pausePanel.SetActive(false);
        endPanel.SetActive(false);
        selectPanel.SetActive(true);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    void BuildCanvas()
    {
        var go = new GameObject("HUD Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    RectTransform FullPanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return rt;
    }

    RectTransform Box(Transform parent, string name, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return rt;
    }

    Text Label(Transform parent, string name, string txt, int size, Color color, TextAnchor align, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 boxSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = boxSize;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = txt;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color color, UnityAction onClick, int fontSize = 26)
    {
        var go = new GameObject("Button_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        go.GetComponent<Image>().color = color;

        var btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        btn.colors = colors;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(rt, false);
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(8f, 8f);
        trt.offsetMax = new Vector2(-8f, -8f);
        var t = textGo.GetComponent<Text>();
        t.font = font;
        t.text = label;
        t.fontSize = fontSize;
        t.color = new Color(0.05f, 0.06f, 0.08f);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return btn;
    }

    // ------------------------------------------------------------------ HUD

    void BuildHUD()
    {
        hud = FullPanel(canvas.transform, "HUD", new Color(0f, 0f, 0f, 0f)).gameObject;

        charNameText = Label(hud.transform, "CharName", "EXCALIBUR PRIME", 30, new Color(0.95f, 0.85f, 0.5f), TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -24f), new Vector2(600f, 40f));

        userText = Label(hud.transform, "User", "", 18, new Color(0.65f, 0.72f, 0.62f), TextAnchor.UpperLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(480f, -30f), new Vector2(320f, 30f));
        userText.text = AuthSession.HasSession ? "USER: " + AuthSession.Username : "";

        Label(hud.transform, "HpCaption", "HP", 20, new Color(0.9f, 0.9f, 0.9f), TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -72f), new Vector2(40f, 24f));
        var hpBg = Box(hud.transform, "HpBarBg", new Color(0.08f, 0.09f, 0.11f, 0.85f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -72f), new Vector2(BarWidth, 26f));
        hpFill = Box(hpBg, "HpFill", new Color(0.85f, 0.25f, 0.28f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(BarWidth, 26f));
        hpText = Label(hpBg, "HpText", "150 / 150", 18, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(BarWidth, 26f));

        Label(hud.transform, "EnCaption", "EN", 20, new Color(0.9f, 0.9f, 0.9f), TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -106f), new Vector2(40f, 24f));
        var enBg = Box(hud.transform, "EnBarBg", new Color(0.08f, 0.09f, 0.11f, 0.85f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(72f, -106f), new Vector2(BarWidth, 26f));
        energyFill = Box(enBg, "EnFill", new Color(0.25f, 0.6f, 0.95f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(BarWidth, 26f));
        energyText = Label(enBg, "EnText", "150 / 150", 18, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(BarWidth, 26f));

        waveText = Label(hud.transform, "Wave", "WAVE 1 / 3", 30, new Color(0.9f, 0.95f, 1f), TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -24f), new Vector2(400f, 40f));
        killsText = Label(hud.transform, "Kills", "KILLS 0", 24, new Color(0.8f, 0.85f, 0.9f), TextAnchor.UpperRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -66f), new Vector2(400f, 32f));

        ability1Text = Label(hud.transform, "Ability1", "Q  ABILITY", 22, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-200f, 100f), new Vector2(360f, 90f));
        ability2Text = Label(hud.transform, "Ability2", "E  ABILITY", 22, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(200f, 100f), new Vector2(360f, 90f));

        var hint = Label(hud.transform, "Hint", "A / D move   SPACE jump (double)   SHIFT dash   LMB fire   Q / E abilities   ESC pause", 18,
            new Color(0.75f, 0.8f, 0.85f, 0.9f), TextAnchor.LowerLeft,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 24f), new Vector2(1200f, 30f));
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;

        cross1 = Box(hud.transform, "CrossH1", new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(3f, 18f));
        cross2 = Box(hud.transform, "CrossH2", new Color(1f, 1f, 1f, 0.85f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(18f, 3f));

        messageText = Label(hud.transform, "Message", "", 54, new Color(1f, 0.9f, 0.5f, 0f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(1200f, 80f));
    }

    // ------------------------------------------------------- character select

    void BuildSelectPanel()
    {
        selectPanel = FullPanel(canvas.transform, "CharacterSelect", new Color(0.02f, 0.03f, 0.05f, 0.96f)).gameObject;

        Label(selectPanel.transform, "Title", "WARFRAME", 84, new Color(0.95f, 0.85f, 0.45f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1400f, 100f));
        Label(selectPanel.transform, "Subtitle", "THE OLD WAR   //   SELECT YOUR WARFRAME", 30, new Color(0.7f, 0.85f, 0.95f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1400f, 50f));

        CharacterId[] ids = { CharacterId.ExcaliburPrime, CharacterId.MesaPrime, CharacterId.WispPrime };
        for (int i = 0; i < ids.Length; i++)
        {
            CharacterId id = ids[i];
            CharacterStats s = CharacterStats.Get(id);
            string label = s.displayName + "\n\n" +
                           "HP " + s.maxHealth + "    ENERGY " + s.maxEnergy + "\n" +
                           "SPEED " + s.moveSpeed + "    DMG " + s.damage + "\n\n" +
                           s.weaponName + "\n\n" +
                           "Q  " + s.ability1Name + "\n" +
                           "E  " + s.ability2Name;
            MakeButton(selectPanel.transform, label, new Vector2((i - 1) * 440f, 20f), new Vector2(400f, 420f), s.color, delegate { GameManager.Instance.StartGame(id); }, 22);
        }

        Label(selectPanel.transform, "Footer", "Objective: survive 3 waves of Grineer and Infested attackers.", 22,
            new Color(0.65f, 0.7f, 0.75f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1200f, 40f));
    }

    // ------------------------------------------------------------- pause/end

    void BuildPausePanel()
    {
        pausePanel = FullPanel(canvas.transform, "Pause", new Color(0f, 0f, 0f, 0.75f)).gameObject;
        Label(pausePanel.transform, "Title", "PAUSED", 72, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(800f, 90f));

        MakeButton(pausePanel.transform, "RESUME", new Vector2(0f, 40f), new Vector2(420f, 76f), new Color(0.55f, 0.85f, 0.6f),
            delegate { GameManager.Instance.ResumeGame(); });
        MakeButton(pausePanel.transform, "CHANGE WARFRAME", new Vector2(0f, -60f), new Vector2(420f, 76f), new Color(0.6f, 0.75f, 0.95f),
            delegate { GameManager.Instance.ReturnToMenu(); });
        MakeButton(pausePanel.transform, "QUIT GAME", new Vector2(0f, -160f), new Vector2(420f, 76f), new Color(0.9f, 0.55f, 0.5f),
            delegate { Application.Quit(); });
    }

    void BuildEndPanel()
    {
        endPanel = FullPanel(canvas.transform, "End", new Color(0f, 0f, 0f, 0.85f)).gameObject;
        endTitle = Label(endPanel.transform, "Title", "MISSION COMPLETE", 72, new Color(0.6f, 0.95f, 0.6f), TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(1200f, 90f));
        endBody = Label(endPanel.transform, "Body", "", 30, Color.white, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 100f), new Vector2(1200f, 80f));

        MakeButton(endPanel.transform, "PLAY AGAIN", new Vector2(0f, -40f), new Vector2(420f, 76f), new Color(0.55f, 0.85f, 0.6f),
            delegate { GameManager.Instance.ReturnToMenu(); });
        MakeButton(endPanel.transform, "QUIT GAME", new Vector2(0f, -140f), new Vector2(420f, 76f), new Color(0.9f, 0.55f, 0.5f),
            delegate { Application.Quit(); });
    }

    // ------------------------------------------------------------ show/hide

    public void ShowCharacterSelect()
    {
        hud.SetActive(false);
        pausePanel.SetActive(false);
        endPanel.SetActive(false);
        selectPanel.SetActive(true);
    }

    public void ShowHUD()
    {
        selectPanel.SetActive(false);
        pausePanel.SetActive(false);
        endPanel.SetActive(false);
        hud.SetActive(true);

        PlayerController p = GameManager.Instance != null ? GameManager.Instance.Player : null;
        if (p != null && p.Stats != null)
        {
            charNameText.text = p.Stats.displayName;
            charNameText.color = p.Stats.color;
            ability1Text.text = "Q   " + p.Stats.ability1Name + "\n" + p.Stats.ability1Desc;
            ability2Text.text = "E   " + p.Stats.ability2Name + "\n" + p.Stats.ability2Desc;
        }
        messageText.text = "";
        messageTimer = 0f;
    }

    public void ShowPause()
    {
        pausePanel.SetActive(true);
    }

    public void HidePause()
    {
        pausePanel.SetActive(false);
    }

    public void ShowVictory(int kills)
    {
        endTitle.text = "MISSION COMPLETE";
        endTitle.color = new Color(0.6f, 0.95f, 0.6f);
        endBody.text = "All 3 waves cleared.   Enemies eliminated: " + kills;
        pausePanel.SetActive(false);
        endPanel.SetActive(true);
    }

    public void ShowDefeat(int kills, int wave)
    {
        endTitle.text = "MISSION FAILED";
        endTitle.color = new Color(0.95f, 0.4f, 0.4f);
        endBody.text = "Warframe destroyed on wave " + Mathf.Max(1, wave) + ".   Enemies eliminated: " + kills;
        pausePanel.SetActive(false);
        endPanel.SetActive(true);
    }

    public void SetWave(int wave, int total)
    {
        waveText.text = "WAVE " + wave + " / " + total;
    }

    public void FlashMessage(string text)
    {
        messageText.text = text;
        messageText.color = new Color(1f, 0.9f, 0.5f, 1f);
        messageTimer = 2f;
    }

    // ---------------------------------------------------------------- update

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        if (messageTimer > 0f)
        {
            messageTimer -= Time.unscaledDeltaTime;
            Color c = messageText.color;
            c.a = Mathf.Clamp01(messageTimer);
            messageText.color = c;
            if (messageTimer <= 0f) messageText.text = "";
        }

        if (!hud.activeSelf) return;

        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && cross1 != null)
        {
            Vector2 local;
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mouse.position.ReadValue(), null, out local))
            {
                cross1.anchoredPosition = local;
                cross2.anchoredPosition = local;
            }
        }

        PlayerController p = gm.Player;
        if (p == null || p.Stats == null) return;

        float hpPct = Mathf.Clamp01(p.Health / p.Stats.maxHealth);
        float enPct = Mathf.Clamp01(p.Energy / p.Stats.maxEnergy);
        hpFill.sizeDelta = new Vector2(BarWidth * hpPct, 26f);
        energyFill.sizeDelta = new Vector2(BarWidth * enPct, 26f);
        hpText.text = Mathf.CeilToInt(p.Health) + " / " + Mathf.CeilToInt(p.Stats.maxHealth);
        energyText.text = Mathf.CeilToInt(p.Energy) + " / " + Mathf.CeilToInt(p.Stats.maxEnergy);
        killsText.text = "KILLS " + gm.Kills;

        UpdateAbility(ability1Text, "Q", p.Stats.ability1Name, p.Ability1Left, p.Stats.ability1Cost, p.Energy);
        UpdateAbility(ability2Text, "E", p.Stats.ability2Name, p.Ability2Left, p.Stats.ability2Cost, p.Energy);
    }

    void UpdateAbility(Text label, string key, string name, float left, float cost, float energy)
    {
        string state = left > 0f ? "COOLDOWN " + left.ToString("0.0") + "s" : (energy >= cost ? "READY" : "LOW ENERGY");
        label.text = key + "   " + name + "\n" + state;
        label.color = left > 0f ? new Color(0.6f, 0.6f, 0.65f) : (energy >= cost ? Color.white : new Color(0.85f, 0.6f, 0.4f));
    }
}
