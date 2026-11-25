using UnityEngine;

public static class SpriteUtil
{
    static Sprite square;
    static Sprite circle;

    public static Sprite Square
    {
        get
        {
            if (square == null)
            {
                var tex = new Texture2D(8, 8);
                var px = new Color32[64];
                for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
                tex.SetPixels32(px);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                square = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            }
            return square;
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (circle == null)
            {
                int s = 64;
                var tex = new Texture2D(s, s);
                float r = s * 0.5f;
                for (int y = 0; y < s; y++)
                {
                    for (int x = 0; x < s; x++)
                    {
                        float dx = x - r + 0.5f;
                        float dy = y - r + 0.5f;
                        bool inside = dx * dx + dy * dy <= r * r;
                        tex.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
                    }
                }
                tex.Apply();
                circle = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), s);
            }
            return circle;
        }
    }

    public static SpriteRenderer Rect(Transform parent, string name, Vector2 localPos, Vector2 size, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Square;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }

    public static SpriteRenderer Round(Transform parent, string name, Vector2 localPos, float diameter, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * diameter;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Circle;
        sr.color = color;
        sr.sortingOrder = order;
        return sr;
    }
}
