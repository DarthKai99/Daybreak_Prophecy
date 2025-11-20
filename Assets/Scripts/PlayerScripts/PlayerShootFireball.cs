using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerShootFireball : MonoBehaviour
{
    [Header("Fireball")]
    [SerializeField] private GameObject fireballPrefab; // assign in Inspector
    [SerializeField] private float spawnOffset = 0.6f;  // how far in front of player
    [SerializeField] private float cooldown = 0.25f;    // seconds between shots
    [SerializeField] private int   damage = 2;          // damage per fireball
    [SerializeField] private int   mpCost = 2;          // MP per shot

    private PlayerStats stats;
    private float nextShootTime = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        // Ensure InputSystem and prefab exist
        if (Mouse.current == null || fireballPrefab == null) return;

        // press F (or hold) to shoot
        //isPressed if you want it to hold to shoot
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TryShootTowardMouse();
        }
    }

    void TryShootTowardMouse()
    {
        if (Time.time < nextShootTime) return;
        if (stats && mpCost > 0 && !stats.UseMP(mpCost)) return;

        // Convert screen-space mouse to world-space
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;

        // Compute direction
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // Spawn position slightly in front of the player
        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);

        // Spawn fireball prefab
        var go = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

        // Make sure projectile script exists and initialize
        var proj = go.GetComponent<FireballProjectile>();
        if (!proj) proj = go.AddComponent<FireballProjectile>();
        proj.Init(dir, damage, gameObject);

        nextShootTime = Time.time + cooldown;
    }
}
