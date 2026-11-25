using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterStats Stats;
    public float Health;
    public float Energy;

    Rigidbody2D rb;
    CapsuleCollider2D col;
    Transform gunPivot;
    Transform visualRoot;
    Transform legL;
    Transform legR;
    Transform ribbonL;
    Transform ribbonR;
    Transform tailPivot;
    SpriteRenderer castGlow;
    Vector2 pivotBase;
    float animTime;
    float aimAngle;
    float slashTimer;
    float slashSide = 1f;
    float recoilTimer;
    float castTimer;
    SpriteRenderer[] bodyRenderers;
    Color[] bodyColors;

    float fireTimer;
    float cooldown1;
    float cooldown2;
    float speedBuff;
    float shieldBuff;
    float hitFlash;
    float dashTimer;
    float dashCooldown;
    float dashDir;
    int jumpsLeft = 2;
    bool dead;

    public float Ability1Left { get { return Mathf.Max(0f, cooldown1); } }
    public float Ability2Left { get { return Mathf.Max(0f, cooldown2); } }
    public bool ShieldActive { get { return shieldBuff > 0f; } }
    public bool SpeedActive { get { return speedBuff > 0f; } }
    public Vector3 AimDirection { get; private set; } = Vector3.right;

    public void Init(CharacterStats stats)
    {
        Stats = stats;
        Health = stats.maxHealth;
        Energy = stats.maxEnergy;

        rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3.1f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        col = gameObject.GetComponent<CapsuleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.6f, 1.4f);
        col.offset = new Vector2(0f, 0.7f);

        BuildBody();

        bodyRenderers = GetComponentsInChildren<SpriteRenderer>();
        bodyColors = new Color[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++) bodyColors[i] = bodyRenderers[i].color;
    }

    void BuildBody()
    {
        Color main = Stats.color;
        Color dark = new Color(0.10f, 0.11f, 0.15f);

        var root = new GameObject("Visual");
        root.transform.SetParent(transform, false);
        visualRoot = root.transform;

        SpriteUtil.Round(visualRoot, "Glow", new Vector2(0f, 0.75f), 1.9f, new Color(main.r, main.g, main.b, 0.16f), 8);

        var legs = new GameObject("Legs");
        legs.transform.SetParent(visualRoot, false);
        legs.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        legL = SpriteUtil.Rect(legs.transform, "LegL", new Vector2(-0.13f, -0.25f), new Vector2(0.2f, 0.55f), dark, 10).transform;
        legR = SpriteUtil.Rect(legs.transform, "LegR", new Vector2(0.13f, -0.25f), new Vector2(0.2f, 0.55f), dark, 10).transform;

        SpriteUtil.Rect(visualRoot, "Torso", new Vector2(0f, 0.8f), new Vector2(0.56f, 0.6f), main, 11);
        castGlow = SpriteUtil.Round(visualRoot, "CastGlow", new Vector2(0f, 0.85f), 2.2f, new Color(0.45f, 0.9f, 1f, 0f), 9);
        Color accent = Stats.id == CharacterId.MesaPrime ? new Color(0.95f, 0.72f, 0.28f) : new Color(0.35f, 0.9f, 1f);
        SpriteUtil.Rect(visualRoot, "Chest", new Vector2(0.08f, 0.86f), new Vector2(0.2f, 0.3f), accent, 12);
        SpriteUtil.Round(visualRoot, "Head", new Vector2(0f, 1.22f), 0.44f, main, 13);
        SpriteUtil.Rect(visualRoot, "Visor", new Vector2(0.14f, 1.24f), new Vector2(0.2f, 0.08f), accent, 14);
        SpriteUtil.Rect(visualRoot, "Scarf", new Vector2(-0.28f, 1.0f), new Vector2(0.35f, 0.12f),
            Stats.id == CharacterId.WispPrime ? new Color(0.45f, 0.8f, 1f)
            : Stats.id == CharacterId.ExcaliburPrime ? new Color(0.85f, 0.16f, 0.16f)
            : new Color(0.95f, 0.72f, 0.28f), 9);

        if (Stats.id == CharacterId.ExcaliburPrime)
        {
            // Praefector-style plating: silver shell, black underlayer, red trim, horned helm.
            Color plate = new Color(0.82f, 0.84f, 0.88f);
            Color black = new Color(0.11f, 0.11f, 0.13f);
            Color red = new Color(0.85f, 0.16f, 0.16f);

            var padL = SpriteUtil.Rect(visualRoot, "PadL", new Vector2(-0.36f, 1.02f), new Vector2(0.26f, 0.3f), black, 12);
            padL.transform.localRotation = Quaternion.Euler(0f, 0f, 12f);
            var padR = SpriteUtil.Rect(visualRoot, "PadR", new Vector2(0.36f, 1.02f), new Vector2(0.26f, 0.3f), black, 12);
            padR.transform.localRotation = Quaternion.Euler(0f, 0f, -12f);

            var hornL = SpriteUtil.Rect(visualRoot, "HornL", new Vector2(-0.2f, 1.5f), new Vector2(0.10f, 0.32f), plate, 15);
            hornL.transform.localRotation = Quaternion.Euler(0f, 0f, 16f);
            var hornR = SpriteUtil.Rect(visualRoot, "HornR", new Vector2(0.2f, 1.5f), new Vector2(0.10f, 0.32f), plate, 15);
            hornR.transform.localRotation = Quaternion.Euler(0f, 0f, -16f);

            SpriteUtil.Rect(visualRoot, "TrimTorso", new Vector2(0f, 0.72f), new Vector2(0.44f, 0.06f), red, 12);
            SpriteUtil.Rect(visualRoot, "TrimChest", new Vector2(0f, 0.98f), new Vector2(0.34f, 0.05f), red, 13);
            SpriteUtil.Rect(visualRoot, "LegPlateL", new Vector2(-0.13f, 0.35f), new Vector2(0.24f, 0.5f), plate, 9);
            SpriteUtil.Rect(visualRoot, "LegPlateR", new Vector2(0.13f, 0.35f), new Vector2(0.24f, 0.5f), plate, 9);
            SpriteUtil.Rect(visualRoot, "LegTrimL", new Vector2(-0.13f, 0.35f), new Vector2(0.05f, 0.5f), red, 10);
            SpriteUtil.Rect(visualRoot, "LegTrimR", new Vector2(0.13f, 0.35f), new Vector2(0.05f, 0.5f), red, 10);
        }

        if (Stats.id == CharacterId.MesaPrime)
        {
            // Gunslinger look: wide hat, ears, gold trim, skirt panels, tail.
            Color hat = new Color(0.15f, 0.14f, 0.18f);
            SpriteUtil.Rect(visualRoot, "HatBrim", new Vector2(0f, 1.5f), new Vector2(1.05f, 0.14f), hat, 16);
            SpriteUtil.Rect(visualRoot, "HatCrown", new Vector2(0f, 1.64f), new Vector2(0.5f, 0.3f), hat, 15);
            SpriteUtil.Rect(visualRoot, "HatBand", new Vector2(0f, 1.54f), new Vector2(0.52f, 0.05f), accent, 17);
            var earL = SpriteUtil.Rect(visualRoot, "EarL", new Vector2(-0.26f, 1.82f), new Vector2(0.16f, 0.3f), new Color(0.8f, 0.8f, 0.86f), 17);
            earL.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            var earR = SpriteUtil.Rect(visualRoot, "EarR", new Vector2(0.26f, 1.82f), new Vector2(0.16f, 0.3f), new Color(0.8f, 0.8f, 0.86f), 17);
            earR.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);

            SpriteUtil.Rect(visualRoot, "SkirtL", new Vector2(-0.3f, 0.42f), new Vector2(0.2f, 0.9f), hat, 8);
            SpriteUtil.Rect(visualRoot, "SkirtR", new Vector2(0.3f, 0.42f), new Vector2(0.2f, 0.9f), hat, 8);
            SpriteUtil.Rect(visualRoot, "SkirtTrim", new Vector2(0f, 0.28f), new Vector2(0.72f, 0.05f), accent, 9);

            var t = new GameObject("TailPivot");
            t.transform.SetParent(visualRoot, false);
            t.transform.localPosition = new Vector3(-0.2f, 0.5f, 0f);
            tailPivot = t.transform;
            SpriteUtil.Rect(tailPivot, "Tail", new Vector2(-0.55f, 0f), new Vector2(1.1f, 0.2f), new Color(0.82f, 0.82f, 0.88f), 6);
            SpriteUtil.Rect(tailPivot, "TailTip", new Vector2(-1.05f, 0.02f), new Vector2(0.35f, 0.14f), new Color(0.95f, 0.95f, 1f), 6);
        }

        if (Stats.id == CharacterId.WispPrime)
        {
            // Wisp-style flowing cloth panels
            ribbonL = SpriteUtil.Rect(visualRoot, "RibbonL", new Vector2(-0.32f, 0.6f), new Vector2(0.16f, 1.15f), new Color(0.92f, 0.96f, 1f), 7).transform;
            ribbonR = SpriteUtil.Rect(visualRoot, "RibbonR", new Vector2(0.32f, 0.6f), new Vector2(0.16f, 1.15f), new Color(0.42f, 0.78f, 1f), 7).transform;
            SpriteUtil.Rect(visualRoot, "Crest", new Vector2(0f, 1.5f), new Vector2(0.1f, 0.32f), new Color(0.92f, 0.96f, 1f), 14);
        }

        var pivot = new GameObject("GunPivot");
        pivot.transform.SetParent(visualRoot, false);
        pivot.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        gunPivot = pivot.transform;
        pivotBase = gunPivot.localPosition;
        if (Stats.meleePrimary)
        {
            SpriteUtil.Rect(gunPivot, "BladeGlow", new Vector2(0.6f, 0f), new Vector2(1.25f, 0.22f), new Color(0.4f, 0.9f, 1f, 0.35f), 14);
            SpriteUtil.Rect(gunPivot, "Blade", new Vector2(0.62f, 0f), new Vector2(1.15f, 0.09f), new Color(0.85f, 0.96f, 1f), 15);
            SpriteUtil.Rect(gunPivot, "Hilt", new Vector2(0.12f, 0f), new Vector2(0.22f, 0.12f), dark, 16);
        }
        else
        {
            SpriteUtil.Rect(gunPivot, "Gun", new Vector2(0.45f, 0f), new Vector2(0.7f, 0.14f), dark, 15);
            SpriteUtil.Rect(gunPivot, "GunGlow", new Vector2(0.8f, 0f), new Vector2(0.12f, 0.1f), accent, 16);
            if (Stats.id == CharacterId.MesaPrime)
            {
                SpriteUtil.Rect(gunPivot, "Gun2", new Vector2(0.42f, -0.17f), new Vector2(0.62f, 0.12f), dark, 15);
                SpriteUtil.Rect(gunPivot, "GunGlow2", new Vector2(0.72f, -0.17f), new Vector2(0.1f, 0.09f), accent, 16);
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        float dt = Time.deltaTime;
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        bool grounded = IsGrounded();
        if (grounded) jumpsLeft = 2;

        // Aim toward mouse
        Vector3 aim = AimPoint();
        AimDirection = (aim - (transform.position + Vector3.up * 0.85f)).normalized;
        if (gunPivot != null)
        {
            aimAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            gunPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle);
            gunPivot.localScale = new Vector3(1f, AimDirection.x < 0f ? -1f : 1f, 1f);
        }

        // Horizontal movement
        float ix = 0f;
        if (kb.aKey.isPressed) ix -= 1f;
        if (kb.dKey.isPressed) ix += 1f;

        float speed = Stats.moveSpeed * (speedBuff > 0f ? 1.3f : 1f);

        if (fireTimer > 0f) fireTimer -= dt;
        if (cooldown1 > 0f) cooldown1 -= dt;
        if (cooldown2 > 0f) cooldown2 -= dt;
        if (speedBuff > 0f) speedBuff -= dt;
        if (shieldBuff > 0f) shieldBuff -= dt;
        if (dashCooldown > 0f) dashCooldown -= dt;
        if (dashTimer > 0f) dashTimer -= dt;

        // Dash
        if (kb.leftShiftKey.wasPressedThisFrame && dashCooldown <= 0f)
        {
            dashTimer = 0.16f;
            dashCooldown = 0.8f;
            dashDir = ix != 0f ? ix : (AimDirection.x >= 0f ? 1f : -1f);
        }

        float vx = dashTimer > 0f ? dashDir * 26f : ix * speed;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);

        // Jump / double jump
        if (kb.spaceKey.wasPressedThisFrame && jumpsLeft > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 16.5f);
            jumpsLeft--;
            if (dashTimer > 0f) dashTimer = 0f;
        }

        Energy = Mathf.Min(Stats.maxEnergy, Energy + Stats.energyRegen * dt);

        if (mouse.leftButton.isPressed && fireTimer <= 0f)
        {
            fireTimer = Stats.fireInterval;
            Shoot(AimDirection);
        }

        if (kb.qKey.wasPressedThisFrame) Ability1();
        if (kb.eKey.wasPressedThisFrame) Ability2();

        Animate(dt, ix, grounded);
        UpdateFlash(dt);
    }

    // Procedural sprite animation: run cycle, air pose, aim lean, slash, recoil, cast glow.
    void Animate(float dt, float ix, bool grounded)
    {
        if (visualRoot == null) return;

        if (slashTimer > 0f) slashTimer -= dt;
        if (recoilTimer > 0f) recoilTimer -= dt;
        if (castTimer > 0f) castTimer -= dt;

        float hs = Mathf.Abs(rb.linearVelocity.x);
        bool moving = grounded && (Mathf.Abs(ix) > 0.01f || hs > 0.6f);
        animTime += dt * (moving ? 9f + hs * 0.5f : 2.6f);

        float bob;
        float lean;
        if (!grounded)
        {
            bob = 0.05f;
            lean = Mathf.Clamp(rb.linearVelocity.x * 0.8f, -9f, 9f);
        }
        else if (moving)
        {
            bob = Mathf.Abs(Mathf.Sin(animTime)) * 0.07f;
            lean = Mathf.Clamp(rb.linearVelocity.x * 1.1f, -8f, 8f);
        }
        else
        {
            bob = Mathf.Sin(animTime) * 0.015f;
            lean = 0f;
        }

        visualRoot.localPosition = new Vector3(0f, bob, 0f);
        visualRoot.localRotation = Quaternion.Euler(0f, 0f, -lean);

        float stride = moving ? Mathf.Sin(animTime) * 0.16f : 0f;
        float lift = moving ? Mathf.Abs(Mathf.Cos(animTime)) * 0.05f : 0f;
        if (legL != null) legL.localPosition = new Vector3(-0.13f + stride, -0.25f + lift, 0f);
        if (legR != null) legR.localPosition = new Vector3(0.13f - stride, -0.25f + lift, 0f);

        // Wisp hovers: no foot contact, legs tuck, cloth panels sway.
        if (Stats.id == CharacterId.WispPrime)
        {
            float hover = 0.12f + Mathf.Sin(Time.time * 2.2f) * 0.04f;
            visualRoot.localPosition = new Vector3(0f, bob + hover, 0f);
            visualRoot.localRotation = Quaternion.Euler(0f, 0f, -lean * 0.5f);
            if (legL != null) legL.localPosition = new Vector3(-0.11f, -0.12f + Mathf.Sin(animTime * 0.7f) * 0.03f, 0f);
            if (legR != null) legR.localPosition = new Vector3(0.11f, -0.12f - Mathf.Sin(animTime * 0.7f) * 0.03f, 0f);

            float sway = Mathf.Sin(Time.time * 2.4f) * 9f;
            if (ribbonL != null) ribbonL.localRotation = Quaternion.Euler(0f, 0f, 10f + sway);
            if (ribbonR != null) ribbonR.localRotation = Quaternion.Euler(0f, 0f, -10f - sway);
        }

        // Mesa tail lags behind movement.
        if (tailPivot != null)
        {
            float lag = Mathf.Clamp(-rb.linearVelocity.x * 1.6f, -18f, 18f);
            float wag = Mathf.Sin(Time.time * 3.2f) * 7f;
            tailPivot.localRotation = Quaternion.Euler(0f, 0f, lag + wag);
        }

        // Blade swing for melee frames, barrel kick for gun frames.
        if (gunPivot != null)
        {
            float offset = 0f;
            if (slashTimer > 0f)
            {
                float k = 1f - Mathf.Clamp01(slashTimer / 0.22f);
                offset += Mathf.Lerp(-55f, 70f, k) * slashSide;
            }
            if (recoilTimer > 0f)
            {
                float k = recoilTimer / 0.12f;
                offset += -8f * k;
                gunPivot.localPosition = pivotBase + new Vector2(-0.12f * k, 0f);
            }
            else
            {
                gunPivot.localPosition = pivotBase;
            }
            gunPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle + offset);
        }

        // Ability charge glow pulse.
        if (castGlow != null)
        {
            float a = castTimer > 0f ? Mathf.Clamp01(castTimer / 0.5f) * 0.55f : 0f;
            Color c = castGlow.color;
            c.a = a;
            castGlow.color = c;
            float s = 2.2f + (castTimer > 0f ? Mathf.Sin(Time.time * 30f) * 0.25f : 0f);
            castGlow.transform.localScale = new Vector3(s, s, 1f);
        }
    }

    bool IsGrounded()
    {
        if (rb.linearVelocity.y > 0.5f) return false;
        Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position + Vector2.down * 0.08f, 0.18f);
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

    void UpdateFlash(float dt)
    {
        if (hitFlash > 0f) hitFlash -= dt;
        if (bodyRenderers == null) return;
        float t = Mathf.Clamp01(hitFlash / 0.18f);
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;
            Color flash = Color.Lerp(bodyColors[i], Color.white, t * 0.8f);
            flash.a = bodyColors[i].a; // hit flash must not turn transparent auras into solid discs
            bodyRenderers[i].color = flash;
        }
    }

    Vector3 AimPoint()
    {
        Camera cam = Camera.main;
        var mouse = Mouse.current;
        if (cam == null || mouse == null) return transform.position + Vector3.right * 10f;
        Vector3 screen = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
        world.z = 0f;
        return world;
    }

    void Shoot(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.right;
        if (Stats.meleePrimary)
        {
            MeleeSlash(direction);
            return;
        }
        Vector3 muzzle = transform.position + Vector3.up * 0.85f + direction.normalized * 0.85f;
        Projectile.Spawn(muzzle, direction, Stats.projectileSpeed, Stats.damage, true, new Color(0.35f, 0.95f, 1f), 0.14f, 0f, false);
        recoilTimer = 0.12f;
    }

    void MeleeSlash(Vector3 direction)
    {
        slashTimer = 0.22f;
        slashSide = direction.x >= 0f ? 1f : -1f;
        Vector3 center = transform.position + Vector3.up * 0.8f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, 2.4f);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponentInParent<Enemy>();
            if (e == null) continue;
            Vector3 to = e.transform.position - center;
            if (Vector3.Angle(direction, to) < 95f) e.TakeDamage(Stats.damage);
        }
        Effect.Ring(center + direction.normalized * 1.1f, new Color(1f, 0.85f, 0.4f), 2.4f, 0.22f);
        rb.linearVelocity = new Vector2(direction.x * 6f, rb.linearVelocity.y);
    }

    bool TrySpend(float cost)
    {
        if (Energy < cost) return false;
        Energy -= cost;
        return true;
    }

    void Ability1()
    {
        if (cooldown1 > 0f) return;
        castTimer = 0.5f;
        switch (Stats.id)
        {
            case CharacterId.ExcaliburPrime:
                if (!TrySpend(Stats.ability1Cost)) return;
                cooldown1 = Stats.ability1Cooldown;
                RadialBlind();
                break;
            case CharacterId.MesaPrime:
                if (!TrySpend(Stats.ability1Cost)) return;
                cooldown1 = Stats.ability1Cooldown;
                StartCoroutine(Peacemaker());
                break;
            case CharacterId.WispPrime:
                if (!TrySpend(Stats.ability1Cost)) return;
                cooldown1 = Stats.ability1Cooldown;
                SolGate();
                break;
        }
    }

    void Ability2()
    {
        if (cooldown2 > 0f) return;
        castTimer = 0.5f;
        switch (Stats.id)
        {
            case CharacterId.ExcaliburPrime:
                if (!TrySpend(Stats.ability2Cost)) return;
                cooldown2 = Stats.ability2Cooldown;
                ExaltedBlade();
                break;
            case CharacterId.MesaPrime:
                if (!TrySpend(Stats.ability2Cost)) return;
                cooldown2 = Stats.ability2Cooldown;
                shieldBuff = 8f;
                break;
            case CharacterId.WispPrime:
                if (!TrySpend(Stats.ability2Cost)) return;
                cooldown2 = Stats.ability2Cooldown;
                Health = Mathf.Min(Stats.maxHealth, Health + 40f);
                speedBuff = 8f;
                break;
        }
    }

    void RadialBlind()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 0.8f, 8f);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponentInParent<Enemy>();
            if (e != null) { e.TakeDamage(45f); e.Stun(2.5f); }
        }
        Effect.Ring(transform.position + Vector3.up * 0.8f, new Color(1f, 0.95f, 0.5f), 8f, 0.45f);
    }

    void ExaltedBlade()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + Vector3.up * 0.8f, 6f);
        for (int i = 0; i < hits.Length; i++)
        {
            Enemy e = hits[i].GetComponentInParent<Enemy>();
            if (e == null) continue;
            Vector3 to = e.transform.position - (transform.position + Vector3.up * 0.8f);
            if (Vector3.Angle(AimDirection, to) < 80f) e.TakeDamage(90f);
        }
        Effect.Ring(transform.position + Vector3.up * 0.8f, new Color(0.4f, 0.9f, 1f), 6f, 0.35f);
    }

    IEnumerator Peacemaker()
    {
        for (int i = 0; i < 12; i++)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) yield break;
            Enemy target = Enemy.Nearest(transform.position);
            if (target != null)
            {
                Vector3 muzzle = transform.position + Vector3.up * 0.85f;
                Vector3 dir = (target.AimPoint() - muzzle).normalized;
                Projectile.Spawn(muzzle, dir, 60f, 25f, true, new Color(1f, 0.7f, 0.3f), 0.15f, 0f, false);
                recoilTimer = 0.08f;
            }
            yield return new WaitForSeconds(0.08f);
        }
    }

    void SolGate()
    {
        Vector3 muzzle = transform.position + Vector3.up * 0.85f + AimDirection * 0.9f;
        Projectile.Spawn(muzzle, AimDirection, 45f, 120f, true, new Color(0.5f, 1f, 0.7f), 0.75f, 2f, true);
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        if (shieldBuff > 0f) amount *= 0.4f;
        Health -= amount;
        hitFlash = 0.18f;
        if (Health <= 0f)
        {
            Health = 0f;
            dead = true;
            Effect.Burst(transform.position + Vector3.up * 0.8f, Stats.color);
            if (GameManager.Instance != null) GameManager.Instance.OnPlayerDeath();
            Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(Stats.maxHealth, Health + amount);
    }
}
