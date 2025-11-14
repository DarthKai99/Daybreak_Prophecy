using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private float speed = 16f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private LayerMask enemyMask; // set to Enemy layer(s) in Inspector

    private Rigidbody2D rb;
    private Collider2D col;
    private int damage = 1;
    private GameObject owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Bullet physics
        rb.bodyType = RigidbodyType2D.Dynamic;   // ensure Dynamic
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col.isTrigger = true; // bullets as triggers are simplest
    }

    /// Launch the bullet
    public void Init(Vector2 dir, int dmg, GameObject ownerGO, float? overrideSpeed = null, float? overrideLifetime = null)
    {
        damage = dmg;
        owner = ownerGO;

        // Don’t collide with the shooter
        foreach (var oc in owner.GetComponentsInChildren<Collider2D>())
            Physics2D.IgnoreCollision(col, oc, true);

        float v = overrideSpeed ?? speed;
        float life = overrideLifetime ?? lifetime;

        rb.linearVelocity = dir.normalized * v;   // IMPORTANT: use velocity
        Destroy(gameObject, life);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || other.gameObject == owner) return;

        // Hit enemy by layer?
        bool isEnemyLayer = (enemyMask.value & (1 << other.gameObject.layer)) != 0;

        if (isEnemyLayer)
        {
            // Works even if collider is on a child
            var enemy = other.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        // Hit a solid wall? (non-trigger)
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
