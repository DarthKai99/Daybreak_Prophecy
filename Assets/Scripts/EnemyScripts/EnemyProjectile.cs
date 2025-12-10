using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 2.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private int damage;
    private Vector2 dir;
    private float speed;

    private bool hasHit = false;   // <<< prevents double-processing

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Kinematic-style bullet: moved by code, not physics forces
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Trigger so it doesn't physically push anything
        col.isTrigger = true;
        col.sharedMaterial = null;
    }

    public void Init(Vector2 direction, float projSpeed, int dmg, GameObject ownerGO)
    {
        owner  = ownerGO;
        damage = dmg;
        dir    = direction.normalized;
        speed  = projSpeed;

        rb.linearVelocity = dir * speed;

        // auto-destroy after lifetime in case it never hits anything
        Destroy(gameObject, lifetime);
    }

    // Optional overload
    public void Init(Vector2 direction, int dmg, GameObject ownerGO)
    {
        Init(direction, speed == 0f ? 12f : speed, dmg, ownerGO);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (!other) return;

        GameObject go = other.gameObject;

        // 1) Never hit the owner
        if (go == owner)
            return;

        // --- NEW: projectile vs projectile cancel ---
        var playerBullet   = other.GetComponentInParent<AttackHitbox>();
        var playerFireball = other.GetComponentInParent<FireballProjectile>();

        if (playerBullet != null || playerFireball != null)
        {
            hasHit = true;
            // destroy the other projectile AND this one
            Destroy(go);
            Destroy(gameObject);
            return;
        }
        // --- END NEW ---

        // 2) Player: damage + destroy
        if (go.CompareTag("Player"))
        {
            var ps = go.GetComponentInParent<PlayerStats>();
            if (ps) ps.TakeDamage(damage);

            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // 3) Enemy: ignore and keep flying (no damage)
        if (go.CompareTag("Enemy"))
        {
            return;
        }

        // 4) Everything else (walls, ground, props, etc.) -> destroy
        hasHit = true;
        Destroy(gameObject);
    }

}

