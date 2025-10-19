using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats; // assign the Player in the scene

    [Header("Bars")]
    public BarUI hpBar;
    public BarUI mpBar;
    public BarUI xpBar;

    void Start()
    {
        if (!stats) stats = FindFirstObjectByType<PlayerStats>();

        // Init maxes
        if (hpBar) hpBar.SetMax(GetMaxHP());
        if (mpBar) mpBar.SetMax(GetMaxMP());
        if (xpBar) xpBar.SetMax(GetXPToNext());

        // Push current values once (Start in PlayerStats also pushes, this is just safety)
        if (hpBar) hpBar.SetValue(GetHP());
        if (mpBar) mpBar.SetValue(GetMP());
        if (xpBar) xpBar.SetValue(GetXP());

        // Subscribe to stat events
        stats.OnHPChanged  += (curr, max) => { if (hpBar) { hpBar.SetMax(max); hpBar.SetValue(curr); } };
        stats.OnMPChanged  += (curr, max) => { if (mpBar) { mpBar.SetMax(max); mpBar.SetValue(curr); } };
        stats.OnXPChanged  += (curr, toNext) => { if (xpBar) { xpBar.SetMax(toNext); xpBar.SetValue(curr); } };
        stats.OnLevelChanged += lvl => {
            // if you want to show level text later, do it here
        };
    }

    // Helpers to read private fields via properties if you later expose them
    // For now, we just cache via the initial events; these are placeholders
    int GetHP()       => 0; // not used after events are wired
    int GetMaxHP()    => 100;
    int GetMP()       => 0;
    int GetMaxMP()    => 100;
    int GetXP()       => 0;
    int GetXPToNext() => 25;
}
