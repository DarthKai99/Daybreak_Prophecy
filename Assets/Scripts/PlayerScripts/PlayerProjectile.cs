using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask enemyMask; // set to Enemy layer(s) in Inspector

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Physics setup
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col.isTrigger = true; // projectiles work best as triggers
    }

    public void Init(Vector2 dir, int dmg, GameObject ownerGO)
    {
        owner = ownerGO;
        damage = dmg;

        // Ignore hitting the shooter
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || other.gameObject == owner) return;

        // Only interact with Enemy layer(s)
        if ((enemyMask.value & (1 << other.gameObject.layer)) == 0)
        {
            // If it's a solid wall/platform, despawn
            if (!other.isTrigger) Destroy(gameObject);
            return;
        }

        // Work even if collider is on a child by using GetComponentInParent
        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

}
