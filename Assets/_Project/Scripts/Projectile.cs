using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static readonly List<Projectile> All = new List<Projectile>();

    Vector3 dir;
    float speed;
    float damage;
    float life = 5f;
    float radius = 0.14f;
    float splash;
    bool fromPlayer;
    bool pierce;
    Transform owner;

    public static Projectile Spawn(Vector3 pos, Vector3 direction, float speed, float damage, bool fromPlayer, Color color, float radius, float splash, bool pierce)
    {
        var go = new GameObject(fromPlayer ? "PlayerProjectile" : "EnemyProjectile");
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        var vis = SpriteUtil.Round(go.transform, "Vis", Vector2.zero, radius * 2.4f, color, 15);
        vis.gameObject.transform.localScale = Vector3.one * radius * 2.4f;

        var p = go.AddComponent<Projectile>();
        p.dir = direction.normalized;
        p.speed = speed;
        p.damage = damage;
        p.fromPlayer = fromPlayer;
        p.radius = radius;
        p.splash = splash;
        p.pierce = pierce;
        p.owner = fromPlayer && GameManager.Instance != null && GameManager.Instance.Player != null
            ? GameManager.Instance.Player.transform : null;
        All.Add(p);
        return p;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        float step = speed * Time.deltaTime;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, dir, step);
        if (hit.collider != null && !IsOwner(hit.collider) && !hit.collider.isTrigger)
        {
            if (HandleHit(hit)) return;
        }

        transform.position += dir * step;
        life -= Time.deltaTime;
        if (life <= 0f) Kill();
    }

    bool IsOwner(Collider2D c)
    {
        if (owner == null) return false;
        return c.transform == owner || c.transform.IsChildOf(owner);
    }

    bool HandleHit(RaycastHit2D hit)
    {
        if (fromPlayer)
        {
            Enemy e = hit.collider.GetComponentInParent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(damage);
                if (splash > 0f) Splash(hit.point, e);
                if (!pierce) { Kill(); return true; }
                return false;
            }
            Kill();
            return true;
        }

        // Enemy projectiles ignore other enemies so they do not body-block each other
        if (hit.collider.GetComponentInParent<Enemy>() != null) return false;

        PlayerController p = hit.collider.GetComponentInParent<PlayerController>();
        if (p != null) p.TakeDamage(damage);
        Kill();
        return true;
    }

    void Splash(Vector3 point, Enemy ignore)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, splash);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponentInParent<Enemy>();
            if (e != null && e != ignore) e.TakeDamage(damage * 0.5f);
        }
        Effect.Ring(point, new Color(0.5f, 1f, 0.7f), splash, 0.25f);
    }

    void Kill()
    {
        All.Remove(this);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        All.Remove(this);
    }
}
