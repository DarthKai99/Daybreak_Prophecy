using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Level & XP")]
    [SerializeField] private int level = 1;
    [SerializeField] private int xp = 0;
    [SerializeField] private int xpToNext = 25;

    [Header("Vitals")]
    [SerializeField] private int maxHP = 10;
    [SerializeField] private int hp = 10;
    [SerializeField] private int maxMP = 10;
    [SerializeField] private int mp = 10;

    // Events: (current, max)
    public event Action<int,int> OnHPChanged;
    public event Action<int,int> OnMPChanged;
    // XP uses (current, toNext)
    public event Action<int,int> OnXPChanged;
    public event Action<int>     OnLevelChanged;

    void Start()
    {
        RecomputeMaxes();
        hp = maxHP;
        mp = maxMP;

        // initial pushes to HUD
        OnHPChanged?.Invoke(hp, maxHP);
        OnMPChanged?.Invoke(mp, maxMP);
        OnXPChanged?.Invoke(xp, xpToNext);
        OnLevelChanged?.Invoke(level);
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
            xpToNext += 10;
            RecomputeMaxes();

            // full heal/refill on level-up (optional)
            hp = maxHP;
            mp = maxMP;

            OnLevelChanged?.Invoke(level);
            OnHPChanged?.Invoke(hp, maxHP);
            OnMPChanged?.Invoke(mp, maxMP);
            OnXPChanged?.Invoke(xp, xpToNext);
            Debug.Log($"Level Up! Now level {level}. MaxHP {maxHP}, MaxMP {maxMP}.");
        }

        // push normal XP change (if no level-up) too
        OnXPChanged?.Invoke(xp, xpToNext);
    }

    public void TakeDamage(int amount)
    {
        int old = hp;
        hp = Mathf.Max(0, hp - amount);
        if (hp != old) OnHPChanged?.Invoke(hp, maxHP);

        if (hp == 0)
        {
            Debug.Log("Player died.");
            // TODO: respawn / game over
        }
    }

    public bool UseMP(int amount)
    {
        if (mp < amount) return false;
        mp -= amount;
        OnMPChanged?.Invoke(mp, maxMP);
        return true;
    }

    public void Heal(int amount)
    {
        int old = hp;
        hp = Mathf.Min(maxHP, hp + amount);
        if (hp != old) OnHPChanged?.Invoke(hp, maxHP);
    }

    public void RestoreMP(int amt)
    {
        int old = mp;
        mp = Mathf.Min(maxMP, mp + amt);
        if (mp != old) OnMPChanged?.Invoke(mp, maxMP);
    }
}
