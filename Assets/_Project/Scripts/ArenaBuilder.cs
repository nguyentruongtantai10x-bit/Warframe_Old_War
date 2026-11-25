using UnityEngine;

public static class ArenaBuilder
{
    public const float HalfWidth = 55f;
    public const float FloorY = -4f;
    public const float CeilingY = 12f;

    static GameObject Solid(Transform root, string name, Vector2 pos, Vector2 size, Color color, int order, bool collider)
    {
        var sr = SpriteUtil.Rect(root, name, pos, size, color, order);
        if (collider)
        {
            var bc = sr.gameObject.AddComponent<BoxCollider2D>();
            bc.size = Vector2.one;
        }
        return sr.gameObject;
    }

    static void Neon(Transform root, Vector2 pos, Vector2 size, Color color, int order)
    {
        SpriteUtil.Rect(root, "Neon", pos, size, color, order);
        SpriteUtil.Rect(root, "NeonGlow", pos, size * 3f, new Color(color.r, color.g, color.b, 0.10f), order - 1);
    }

    public static Transform Build()
    {
        var root = new GameObject("Arena").transform;

        Color bgDeep = new Color(0.035f, 0.04f, 0.07f);
        Color bgPanel = new Color(0.07f, 0.08f, 0.13f);
        Color steel = new Color(0.13f, 0.15f, 0.20f);
        Color cyan = new Color(0.25f, 0.85f, 1f);
        Color magenta = new Color(1f, 0.25f, 0.65f);
        float width = HalfWidth * 2f + 20f;

        // Backdrop
        SpriteUtil.Rect(root, "Backdrop", new Vector2(0f, 4f), new Vector2(width, 30f), bgDeep, -100);

        // Distant towers / panels across the whole arena
        for (float x = -HalfWidth + 4f; x <= HalfWidth - 4f; x += 8f)
        {
            int i = Mathf.RoundToInt((x + HalfWidth) / 8f);
            float h = 6f + (i % 3) * 3f;
            SpriteUtil.Rect(root, "Tower", new Vector2(x, FloorY + h * 0.5f + 1f), new Vector2(4.4f, h), bgPanel, -90);
            SpriteUtil.Rect(root, "TowerLight", new Vector2(x, FloorY + h + 1f), new Vector2(4.4f, 0.14f), new Color(0.2f, 0.5f, 0.8f, 0.7f), -89);
        }

        // Magenta sky band
        SpriteUtil.Rect(root, "SkyBand", new Vector2(0f, 2.2f), new Vector2(width, 5.5f), new Color(0.35f, 0.05f, 0.3f, 0.55f), -95);
        SpriteUtil.Rect(root, "SkyBand2", new Vector2(0f, 4.6f), new Vector2(width, 0.18f), new Color(1f, 0.3f, 0.6f, 0.5f), -94);

        // Signboards
        SpriteUtil.Rect(root, "Sign1", new Vector2(-HalfWidth * 0.45f, 3.4f), new Vector2(6f, 1.8f), new Color(0.05f, 0.1f, 0.16f), -85);
        SpriteUtil.Rect(root, "Sign1Text", new Vector2(-HalfWidth * 0.45f, 3.4f), new Vector2(5.2f, 0.16f), cyan, -84);
        SpriteUtil.Rect(root, "Sign2", new Vector2(HalfWidth * 0.45f, 4.2f), new Vector2(7f, 2.2f), new Color(0.14f, 0.04f, 0.10f), -85);
        SpriteUtil.Rect(root, "Sign2Text", new Vector2(HalfWidth * 0.45f, 4.2f), new Vector2(6f, 0.18f), magenta, -84);

        // Floor
        Solid(root, "Floor", new Vector2(0f, FloorY - 1.5f), new Vector2(HalfWidth * 2f, 3f), steel, 0, true);
        Neon(root, new Vector2(0f, FloorY + 0.06f), new Vector2(HalfWidth * 2f, 0.09f), cyan, 3);

        // Floor lane markers, so the wider ground reads as space to run
        for (float x = -HalfWidth + 10f; x <= HalfWidth - 10f; x += 10f)
            SpriteUtil.Rect(root, "Lane", new Vector2(x, FloorY + 0.02f), new Vector2(2.4f, 0.05f), new Color(0.25f, 0.85f, 1f, 0.35f), 2);

        // Side walls
        Solid(root, "WallL", new Vector2(-HalfWidth, 4f), new Vector2(1f, 32f), steel, 2, true);
        Solid(root, "WallR", new Vector2(HalfWidth, 4f), new Vector2(1f, 32f), steel, 2, true);

        // Platforms spread across the full width
        Vector2[] pos =
        {
            new Vector2(-44f, -1.2f), new Vector2(-33f, 1.0f), new Vector2(-22f, -1.2f),
            new Vector2(-12f, 1.0f), new Vector2(0f, 4.2f),
            new Vector2(12f, 1.0f), new Vector2(22f, -1.2f), new Vector2(33f, 1.0f), new Vector2(44f, -1.2f),
            new Vector2(-38f, 3.4f), new Vector2(38f, 3.4f)
        };
        Vector2[] size =
        {
            new Vector2(9f, 0.45f), new Vector2(8f, 0.45f), new Vector2(9f, 0.45f),
            new Vector2(8f, 0.45f), new Vector2(10f, 0.45f),
            new Vector2(8f, 0.45f), new Vector2(9f, 0.45f), new Vector2(8f, 0.45f), new Vector2(9f, 0.45f),
            new Vector2(7f, 0.45f), new Vector2(7f, 0.45f)
        };
        for (int i = 0; i < pos.Length; i++)
        {
            Solid(root, "Platform" + i, pos[i], size[i], new Color(0.16f, 0.19f, 0.25f), 4, true);
            Neon(root, pos[i] + new Vector2(0f, size[i].y * 0.5f + 0.04f), new Vector2(size[i].x, 0.07f), i % 2 == 0 ? cyan : magenta, 5);
        }

        // Crates / props, spread out
        Vector2[] crates =
        {
            new Vector2(-5f, FloorY + 0.6f), new Vector2(6.4f, FloorY + 0.5f),
            new Vector2(-28f, FloorY + 0.55f), new Vector2(27.5f, FloorY + 0.5f),
            new Vector2(-41f, FloorY + 0.6f), new Vector2(40f, FloorY + 0.55f)
        };
        for (int i = 0; i < crates.Length; i++)
        {
            float s = i % 2 == 0 ? 1.2f : 1f;
            Solid(root, "Crate" + i, crates[i], new Vector2(s, s), new Color(0.19f, 0.21f, 0.27f), 6, true);
            SpriteUtil.Rect(root, "CrateGlow" + i, crates[i], new Vector2(s * 0.75f, 0.08f), i % 2 == 0 ? magenta : cyan, 7);
        }

        return root;
    }
}
