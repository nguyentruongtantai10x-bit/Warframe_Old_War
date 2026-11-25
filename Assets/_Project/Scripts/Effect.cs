using System.Collections;
using UnityEngine;

public static class Effect
{
    class Runner : MonoBehaviour { }

    static Runner runner;

    static Runner GetRunner()
    {
        if (runner == null)
        {
            var go = new GameObject("EffectRunner");
            runner = go.AddComponent<Runner>();
        }
        return runner;
    }

    public static void Ring(Vector3 center, Color color, float radius, float duration)
    {
        var go = new GameObject("Ring");
        go.transform.position = new Vector3(center.x, center.y, 0f);
        var sr = SpriteUtil.Round(go.transform, "Vis", Vector2.zero, 1f, color, 30);
        GetRunner().StartCoroutine(AnimateRing(sr, go.transform, radius, duration));
    }

    static IEnumerator AnimateRing(SpriteRenderer sr, Transform tr, float radius, float duration)
    {
        float t = 0f;
        Color c = sr.color;
        while (t < duration && sr != null)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float d = Mathf.Lerp(0.6f, radius * 2f, k);
            tr.localScale = new Vector3(d, d, 1f);
            c.a = Mathf.Lerp(0.55f, 0f, k);
            sr.color = c;
            yield return null;
        }
        if (sr != null) Object.Destroy(sr.gameObject);
    }

    public static void Burst(Vector3 center, Color color)
    {
        for (int i = 0; i < 6; i++)
        {
            var go = new GameObject("Debris");
            go.transform.position = new Vector3(center.x, center.y, 0f);
            var sr = SpriteUtil.Rect(go.transform, "Vis", Vector2.zero, Vector2.one * 0.2f, color, 31);
            Vector2 dir = Random.insideUnitCircle.normalized;
            GetRunner().StartCoroutine(AnimateDebris(sr, go.transform, dir));
        }
    }

    static IEnumerator AnimateDebris(SpriteRenderer sr, Transform tr, Vector2 dir)
    {
        float life = 0.45f;
        Vector2 vel = dir * 6f;
        while (life > 0f && sr != null)
        {
            life -= Time.deltaTime;
            vel *= 1f - 4f * Time.deltaTime;
            tr.position += (Vector3)(vel * Time.deltaTime);
            tr.localScale = Vector3.one * Mathf.Max(0.02f, 0.2f * life * 2f);
            yield return null;
        }
        if (sr != null) Object.Destroy(sr.gameObject);
    }
}
