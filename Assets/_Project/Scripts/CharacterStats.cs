using UnityEngine;

public enum CharacterId { ExcaliburPrime = 0, MesaPrime = 1, WispPrime = 2 }

public class CharacterStats
{
    public CharacterId id;
    public string displayName;
    public string weaponName;
    public bool meleePrimary;
    public float maxHealth;
    public float maxEnergy;
    public float moveSpeed;
    public float fireInterval;
    public float damage;
    public float projectileSpeed;
    public float energyRegen;
    public Color color;
    public string ability1Name;
    public string ability2Name;
    public float ability1Cost;
    public float ability2Cost;
    public float ability1Cooldown;
    public float ability2Cooldown;
    public string ability1Desc;
    public string ability2Desc;

    public static CharacterStats Get(CharacterId id)
    {
        switch (id)
        {
            case CharacterId.MesaPrime:
                return new CharacterStats
                {
                    id = id,
                    displayName = "MESA PRIME",
                    weaponName = "Regulators (dual pistols)",
                    maxHealth = 120f,
                    maxEnergy = 130f,
                    moveSpeed = 6.5f,
                    fireInterval = 0.09f,
                    damage = 14f,
                    projectileSpeed = 55f,
                    energyRegen = 9f,
                    color = new Color(0.30f, 0.30f, 0.35f),
                    ability1Name = "Peacemaker",
                    ability2Name = "Shatter Shield",
                    ability1Cost = 50f,
                    ability2Cost = 35f,
                    ability1Cooldown = 10f,
                    ability2Cooldown = 12f,
                    ability1Desc = "Auto-fire 12 shots at nearby enemies (25 dmg each).",
                    ability2Desc = "60% damage reduction for 8 seconds."
                };
            case CharacterId.WispPrime:
                return new CharacterStats
                {
                    id = id,
                    displayName = "WISP PRIME",
                    weaponName = "Fulmin (energy rifle)",
                    maxHealth = 100f,
                    maxEnergy = 180f,
                    moveSpeed = 6f,
                    fireInterval = 0.28f,
                    damage = 34f,
                    projectileSpeed = 70f,
                    energyRegen = 14f,
                    color = new Color(0.88f, 0.93f, 0.99f),
                    ability1Name = "Sol Gate",
                    ability2Name = "Reservoirs",
                    ability1Cost = 60f,
                    ability2Cost = 30f,
                    ability1Cooldown = 12f,
                    ability2Cooldown = 9f,
                    ability1Desc = "Fire a piercing sun beam (120 dmg).",
                    ability2Desc = "Heal 40 HP and gain speed for 8 seconds."
                };
            default:
                return new CharacterStats
                {
                    id = CharacterId.ExcaliburPrime,
                    displayName = "EXCALIBUR PRIME",
                    weaponName = "Skana (melee blade)",
                    meleePrimary = true,
                    maxHealth = 150f,
                    maxEnergy = 150f,
                    moveSpeed = 6f,
                    fireInterval = 0.32f,
                    damage = 34f,
                    projectileSpeed = 0f,
                    energyRegen = 10f,
                    color = new Color(0.82f, 0.84f, 0.88f),
                    ability1Name = "Radial Blind",
                    ability2Name = "Exalted Blade",
                    ability1Cost = 40f,
                    ability2Cost = 30f,
                    ability1Cooldown = 6f,
                    ability2Cooldown = 8f,
                    ability1Desc = "Blind and damage all enemies within 8m (45 dmg, stun).",
                    ability2Desc = "Slash a wide arc in front (90 dmg)."
                };
        }
    }
}
