using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShootMissile : MonoBehaviour
{
    [Header("Missile")]
    [SerializeField] private GameObject missilePrefab; // assign Missile prefab here
    [SerializeField] private float spawnOffset = 0.8f;  // how far in front of player
    [SerializeField] private float cooldown = 1.5f;     // seconds between missiles
    [SerializeField] private int damage = 3;            // damage per enemy in explosion
    [SerializeField] private int mpCost = 5;            // MP cost to fire

    private PlayerStats stats;
    private float nextShootTime = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Mouse.current == null || missilePrefab == null) return;

        if (Time.timeScale == 0f) return;  // no firing while paused

        // Press M to fire a missile
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            TryShootMissile();
        }
    }

    void TryShootMissile()
    {
        if (Time.time < nextShootTime) return;

        // Check MP
        if (stats && mpCost > 0 && !stats.UseMP(mpCost))
            return;

        // Mouse position in world
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        // Direction from player to mouse
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // Spawn position slightly in front of player
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        // Spawn missile
        var go = Instantiate(missilePrefab, spawnPos, Quaternion.identity);

        // Ensure it has MissileProjectile
        var missile = go.GetComponent<MissileProjectile>();
        if (!missile) missile = go.AddComponent<MissileProjectile>();

        missile.Init(dir, damage, gameObject);

        nextShootTime = Time.time + cooldown;

        // PLAY MISSILE SFX
        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.missileClip);
        }


    }
}
