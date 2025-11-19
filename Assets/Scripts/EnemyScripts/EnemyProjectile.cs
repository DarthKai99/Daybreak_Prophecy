using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class EnemyProjectile : MonoBehaviour
{
      [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private int damage;

    // store to restore after ignores
    private Vector2 dir;
    private float spawnTime;

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

    public void Init(Vector2 direction, float projSpeed, int dmg, GameObject ownerGO)
    {
        owner  = ownerGO;
        damage = dmg;
        dir    = direction.normalized;
        speed  = projSpeed;
        spawnTime = Time.time;

        // don't hit the shooter
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        rb.linearVelocity = dir * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        var hitCol = c.collider;
        var hitObj = hitCol.gameObject;
        if (hitObj == owner) return;

        // 1) Player? -> damage + destroy
        if (hitObj.CompareTag("Player"))
        {
            var ps = hitObj.GetComponentInParent<PlayerStats>();
            if (ps != null) ps.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2) Enemy? -> ignore and keep flying (no friendly fire)
        if (hitObj.CompareTag("Enemy"))
        {
            Physics2D.IgnoreCollision(col, hitCol, true);
            rb.linearVelocity = dir * speed; // (Unity 2023+: use linearVelocity)
            return;
        }

        // 3) Anything else (wall/obstacle/etc) -> destroy projectile
        Destroy(gameObject);
    }

}

