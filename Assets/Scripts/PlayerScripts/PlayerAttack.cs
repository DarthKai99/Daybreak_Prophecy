using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // your bullet prefab (with AttackHitbox above)
    [SerializeField] private float spawnOffset = 0.6f;
    [SerializeField] private float fireRate = 3f; // shots per second (minigun)
    [SerializeField] private int damage = 1;
    [SerializeField] private int mpCostPerShot = 0;

    private PlayerStats stats;
    private float nextShotTime;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Mouse.current == null || projectilePrefab == null) return;

        bool fireHeld = Mouse.current.leftButton.isPressed; // hold to shoot with LMB
        if (!fireHeld) return;

        if (Time.time >= nextShotTime)
        {
            ShootTowardMouse();
            nextShotTime = Time.time + (1f / Mathf.Max(1f, fireRate));
        }
    }

    void ShootTowardMouse()
    {
        if (stats && mpCostPerShot > 0 && !stats.UseMP(mpCostPerShot)) return;

        // Convert mouse position (screen) to world position
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        // Direction from player to mouse
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // Spawn projectile slightly in front of the player
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // Launch projectile
        var proj = go.GetComponent<FireballProjectile>();
        if (!proj) proj = go.AddComponent<FireballProjectile>();
        proj.Init(dir, damage, gameObject);
    }
}
