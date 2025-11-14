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

    private Player_movement mover;
    private PlayerStats stats;
    private float nextShotTime;

    void Awake()
    {
        mover = GetComponent<Player_movement>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Keyboard.current == null || projectilePrefab == null) return;

        bool fireHeld = Keyboard.current.spaceKey.isPressed; // hold to spray
        if (!fireHeld) return;

        if (Time.time >= nextShotTime)
        {
            ShootOnce();
            nextShotTime = Time.time + (1f / Mathf.Max(1f, fireRate));
        }
    }

    void ShootOnce()
    {
        if (stats && mpCostPerShot > 0 && !stats.UseMP(mpCostPerShot)) return;

        Vector2 dir = (mover && mover.FacingDir != Vector2.zero) ? mover.FacingDir : Vector2.right;
        dir.Normalize();

        Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);
        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var bullet = go.GetComponent<AttackHitbox>(); // ← our new moving bullet
        if (!bullet) bullet = go.AddComponent<AttackHitbox>();

        bullet.Init(dir, damage, gameObject);
    }
}
