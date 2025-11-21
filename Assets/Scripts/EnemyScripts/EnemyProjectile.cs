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

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        col.isTrigger = false;        // solid so it hits walls
        col.sharedMaterial = null;    // no bounce material
    }

    // This is the signature your EnemyRanged calls
    public void Init(Vector2 direction, float projSpeed, int dmg, GameObject ownerGO)
    {
        owner  = ownerGO;
        damage = dmg;
        dir    = direction.normalized;
        speed  = projSpeed;

        // ignore the shooter's colliders
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        rb.linearVelocity = dir * speed;
        Destroy(gameObject, lifetime);
    }

    // Optional overload (lets you call Init(dir, dmg, owner) if you want)
    public void Init(Vector2 direction, int dmg, GameObject ownerGO)
    {
        Init(direction, speed == 0f ? 12f : speed, dmg, ownerGO);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        var hit = c.collider;
        var go = hit.gameObject;
        if (go == owner) return;

        // Player? -> damage + destroy
        if (go.CompareTag("Player"))
        {
            var ps = go.GetComponentInParent<PlayerStats>();
            if (ps) ps.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Player bullet? -> destroy THIS projectile (leave the player bullet)
        if (go.GetComponentInParent<FireballProjectile>() != null || go.CompareTag("bullet"))
        {
            Destroy(gameObject);
            return;
        }

        // Enemy? -> ignore and keep flying
        if (go.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(col, hit, true);
            rb.linearVelocity = dir * speed;
            return;
        }

        // Wall / anything else -> destroy
        Destroy(gameObject);
    }

}

