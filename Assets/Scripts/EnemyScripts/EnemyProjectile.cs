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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Kinematic-style bullet: moved by code, not physics forces
        rb.bodyType = RigidbodyType2D.Kinematic;    // <-- use bodyType instead of isKinematic
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

    // if you want to call Init(dir, dmg, owner) without speed:
    public void Init(Vector2 direction, int dmg, GameObject ownerGO)
    {
        Init(direction, speed == 0f ? 12f : speed, dmg, ownerGO);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject go = other.gameObject;

        // 1) Never hit the owner
        if (go == owner)
            return;

        // 2) Player: damage + destroy
        if (go.CompareTag("Player"))
        {
            var ps = go.GetComponentInParent<PlayerStats>();
            if (ps) ps.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 3) Enemy: ignore and keep flying (no damage)
        if (go.CompareTag("Enemy"))
        {
            // just ignore this overlap
            return;
        }

        // 4) Player bullet: destroy this projectile
        //    (adjust tag / component name to match your project)
        if (go.CompareTag("bullet") || go.GetComponentInParent<FireballProjectile>() != null)
        {
            Destroy(gameObject);
            return;
        }

        // 5) Everything else (walls, ground, props, etc.) -> destroy
        Destroy(gameObject);
    }


}

