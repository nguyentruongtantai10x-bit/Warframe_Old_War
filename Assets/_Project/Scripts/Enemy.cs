using System.Collections.Generic;
using UnityEngine;

public enum EnemyType { Lancer, Charger }

public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public EnemyType Type;
    public float Health;
    public float MaxHealth;

    float speed;
    float damage;
    float attackInterval;
    float attackRange;
    bool ranged;
    float keepDistance;
    float attackTimer;
    float stunTimer;
    float flashTimer;
    float jumpTimer;
    Rigidbody2D rb;
    SpriteRenderer[] renderers;
    Color[] baseColors;
    Transform barFill;

    public static Enemy Nearest(Vector3 pos)
    {
        Enemy best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i] == null) continue;
            float d = (All[i].transform.position - pos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = All[i]; }
        }
        return best;
    }

    public Vector3 AimPoint()
    {
        return transform.position + Vector3.up * 0.7f;
    }

    public void Init(EnemyType type, int wave)
    {
        Type = type;
        float scale = 1f + (wave - 1) * 0.25f;

        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.1f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = gameObject.GetComponent<CapsuleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.7f, 1.3f);
        col.offset = new Vector2(0f, 0.65f);

        if (type == EnemyType.Lancer)
        {
            MaxHealth = 70f * scale;
            speed = 2.4f;
            damage = 10f;
            attackInterval = 1.8f;
            attackRange = 14f;
            ranged = true;
            keepDistance = 8f;
            BuildLancer();
        }
        else
        {
            MaxHealth = 55f * scale;
            speed = 4.2f;
            damage = 12f;
            attackInterval = 1.1f;
            attackRange = 1.8f;
            ranged = false;
            keepDistance = 0f;
            BuildCharger();
        }
        Health = MaxHealth;

        renderers = GetComponentsInChildren<SpriteRenderer>();
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) baseColors[i] = renderers[i].color;

        BuildHealthBar();
        All.Add(this);
    }

    void BuildLancer()
    {
        Color armor = new Color(0.85f, 0.25f, 0.2f);
        Color dark = new Color(0.16f, 0.12f, 0.14f);
        SpriteUtil.Round(transform, "Glow", new Vector2(0f, 0.7f), 1.8f, new Color(1f, 0.25f, 0.2f, 0.14f), 7);
        SpriteUtil.Rect(transform, "Legs", new Vector2(0f, 0.28f), new Vector2(0.44f, 0.56f), dark, 10);
        SpriteUtil.Rect(transform, "Torso", new Vector2(0f, 0.75f), new Vector2(0.62f, 0.62f), armor, 11);
        SpriteUtil.Rect(transform, "Plate", new Vector2(0.05f, 0.8f), new Vector2(0.22f, 0.34f), new Color(1f, 0.55f, 0.15f), 12);
        SpriteUtil.Round(transform, "Head", new Vector2(0f, 1.2f), 0.42f, dark, 13);
        SpriteUtil.Rect(transform, "Eye", new Vector2(0.12f, 1.22f), new Vector2(0.16f, 0.07f), new Color(1f, 0.65f, 0.15f), 14);
        SpriteUtil.Rect(transform, "Gun", new Vector2(0.5f, 0.75f), new Vector2(0.62f, 0.12f), dark, 14);
    }

    void BuildCharger()
    {
        Color flesh = new Color(0.62f, 0.25f, 0.72f);
        SpriteUtil.Round(transform, "Glow", new Vector2(0f, 0.55f), 1.7f, new Color(0.8f, 0.3f, 1f, 0.14f), 7);
        SpriteUtil.Round(transform, "Body", new Vector2(0f, 0.55f), 1.15f, flesh, 10);
        SpriteUtil.Round(transform, "Head", new Vector2(0.32f, 0.72f), 0.6f, new Color(0.75f, 0.35f, 0.8f), 11);
        SpriteUtil.Rect(transform, "Jaw", new Vector2(0.5f, 0.6f), new Vector2(0.3f, 0.14f), new Color(0.95f, 0.9f, 0.7f), 12);
        for (int i = 0; i < 3; i++)
            SpriteUtil.Rect(transform, "Spike", new Vector2(-0.35f - i * 0.14f, 0.85f - i * 0.22f), new Vector2(0.3f, 0.1f), new Color(0.9f, 0.85f, 0.7f), 9);
    }

    void BuildHealthBar()
    {
        SpriteUtil.Rect(transform, "BarBack", new Vector2(0f, 1.75f), new Vector2(1.1f, 0.12f), new Color(0.05f, 0.05f, 0.07f), 20);
        var fill = SpriteUtil.Rect(transform, "BarFill", new Vector2(0f, 1.75f), new Vector2(1.02f, 0.08f),
            Type == EnemyType.Lancer ? new Color(1f, 0.4f, 0.25f) : new Color(0.85f, 0.4f, 1f), 21);
        barFill = fill.transform;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        PlayerController player = GameManager.Instance.Player;
        if (player == null) return;

        float dt = Time.deltaTime;
        if (flashTimer > 0f) flashTimer -= dt;
        if (stunTimer > 0f) stunTimer -= dt;
        if (attackTimer > 0f) attackTimer -= dt;
        if (jumpTimer > 0f) jumpTimer -= dt;

        UpdateVisuals();
        UpdateBar();

        if (stunTimer > 0f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Vector3 to = player.transform.position - transform.position;
        float dist = to.magnitude;
        Vector3 dir = dist > 0.001f ? to / dist : Vector3.right;
        bool playerAbove = player.transform.position.y > transform.position.y + 1.6f;

        float moveX = 0f;
        if (playerAbove)
        {
            // Raised platforms must not make the player untouchable.
            moveX = Mathf.Abs(dir.x) < 0.15f ? Mathf.Sign(transform.position.x - player.transform.position.x) * -0.5f : dir.x;
        }
        else if (ranged)
        {
            if (dist > keepDistance + 1.5f) moveX = dir.x;
            else if (dist < keepDistance - 2f) moveX = -dir.x;
        }
        else if (dist > attackRange - 0.2f)
        {
            moveX = dir.x;
        }

        for (int i = 0; i < All.Count; i++)
        {
            Enemy other = All[i];
            if (other == null || other == this) continue;
            float dx = transform.position.x - other.transform.position.x;
            if (Mathf.Abs(dx) < 1.2f && Mathf.Abs(dx) > 0.001f) moveX += Mathf.Sign(dx) * 0.4f;
        }

        moveX = Mathf.Clamp(moveX, -1f, 1f);
        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        if (playerAbove && jumpTimer <= 0f && Mathf.Abs(to.x) < 10f && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 17f);
            jumpTimer = 1.5f;
        }

        if (dist <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackInterval;
            Attack(player, dir);
        }
    }

    bool IsGrounded()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.down * 0.1f, 0.2f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null || c.isTrigger) continue;
            if (c.transform == transform || c.transform.IsChildOf(transform)) continue;
            if (c.GetComponentInParent<Enemy>() != null) continue;
            return true;
        }
        return false;
    }

    void Attack(PlayerController player, Vector3 dir)
    {
        if (ranged)
        {
            Vector3 muzzle = transform.position + Vector3.up * 0.75f + dir * 0.6f;
            Projectile.Spawn(muzzle, dir, 26f, damage, false, new Color(1f, 0.55f, 0.2f), 0.16f, 0f, false);
        }
        else
        {
            player.TakeDamage(damage);
        }
    }

    void UpdateVisuals()
    {
        float t = Mathf.Clamp01(flashTimer / 0.12f);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].color = Color.Lerp(baseColors[i], Color.white, t);
        }
    }

    void UpdateBar()
    {
        if (barFill == null) return;
        float pct = Mathf.Clamp01(Health / Mathf.Max(1f, MaxHealth));
        barFill.localScale = new Vector3(1.02f * pct, 0.08f, 1f);
        barFill.localPosition = new Vector3(-0.51f * (1f - pct), 1.75f, 0f);
    }

    public void Stun(float seconds)
    {
        stunTimer = Mathf.Max(stunTimer, seconds);
    }

    public void TakeDamage(float amount)
    {
        if (Health <= 0f) return;
        Health -= amount;
        flashTimer = 0.12f;
        if (Health <= 0f) Die();
    }

    void Die()
    {
        Health = 0f;
        Effect.Burst(transform.position + Vector3.up * 0.7f, Type == EnemyType.Lancer ? new Color(0.9f, 0.35f, 0.25f) : new Color(0.7f, 0.35f, 0.85f));
        All.Remove(this);
        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled(this);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        All.Remove(this);
    }
}
