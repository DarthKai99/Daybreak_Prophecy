using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float wallGraceTime = 0.05f; // ignore walls for first frames

    Rigidbody2D rb;
    Collider2D col;
    GameObject owner;
    int damage;

    Vector2 travelDir;
    float travelSpeed;
    float spawnTime;

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

        col.isTrigger = false; // solid bullet
    }

    public void Init(Vector2 dir, float speed, int dmg, GameObject ownerGO)
    {
        owner = ownerGO;
        damage = dmg;
        travelDir = dir.normalized;
        travelSpeed = speed;
        spawnTime = Time.time;

        // Don’t hit the shooter
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        rb.linearVelocity = travelDir * travelSpeed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        var other = c.collider;
        var otherGO = other.gameObject;
        if (otherGO == owner) return;

        // 1) Player? -> damage + destroy
        var ps = otherGO.GetComponentInParent<PlayerStats>();
        if (ps != null)
        {
            ps.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2) Enemy? -> ignore and keep flying (no friendly fire)
        var enemy = otherGO.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            Physics2D.IgnoreCollision(col, other, true);
            rb.linearVelocity = travelDir * travelSpeed;
            return;
        }

        // 3) Wall/obstacle
        // Allow a tiny grace after spawn so bullets that spawn kissing a wall don't die instantly
        if (Time.time - spawnTime < wallGraceTime)
        {
            Physics2D.IgnoreCollision(col, other, true);
            rb.linearVelocity = travelDir * travelSpeed;

            Destroy(gameObject);
            return;
        }

    }
}
