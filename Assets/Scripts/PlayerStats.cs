using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Level & XP")]
    [SerializeField] private int level = 1;
    [SerializeField] private int xp = 0;
    [SerializeField] private int xpToNext = 25; // simple curve: grows each level

    [Header("Vitals")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int hp = 10;
    [SerializeField] private int maxMP = 10;
    [SerializeField] private int mp = 10;

    void Start()
    {
        RecomputeMaxes();
        hp = maxHP;
        mp = maxMP;
    }

    // Scale max HP/MP so L1=10 -> L10=100
    void RecomputeMaxes()
    {
        float t = Mathf.Clamp01((level - 1) / 9f); // 0 at L1, 1 at L10
        maxHP = Mathf.RoundToInt(Mathf.Lerp(10, 100, t));
        maxMP = Mathf.RoundToInt(Mathf.Lerp(10, 100, t));
        hp = Mathf.Clamp(hp, 0, maxHP);
        mp = Mathf.Clamp(mp, 0, maxMP);
    }

    public void AddXP(int amount)
    {
        xp += amount;
        while (xp >= xpToNext)
        {
            xp -= xpToNext;
            level++;
            // simple growth: +10 XP requirement per level
            xpToNext += 10;
            RecomputeMaxes();
            // Optional: heal a bit or refill MP on level up
            hp = maxHP;
            mp = maxMP;
            Debug.Log($"Level Up! Now level {level}. MaxHP {maxHP}, MaxMP {maxMP}.");
        }
    }

    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
        if (hp == 0)
        {
            Debug.Log("Player died.");
            // TODO: handle death (respawn, game over)
        }
    }

    public bool UseMP(int amount)
    {
        if (mp < amount) return false;
        mp -= amount;
        return true;
    }

    public void Heal(int amount)  { hp = Mathf.Min(maxHP, hp + amount); }
    public void RestoreMP(int amt){ mp = Mathf.Min(maxMP, mp + amt); }
}
