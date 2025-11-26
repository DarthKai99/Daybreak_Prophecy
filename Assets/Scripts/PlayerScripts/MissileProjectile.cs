using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class MissileProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 4f;

    [Header("Homing")]
    [SerializeField] private bool enableHoming = true;
    [SerializeField] private float homingRadius = 5f;      // how far it can "see" enemies
    [SerializeField] private float turnSpeedDeg = 360f;    // degrees per second it can turn

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int damage = 3;

    private Rigidbody2D rb;
    private Collider2D col;
    private GameObject owner;
    private bool hasExploded = false;

    private Transform currentTarget;   // enemy we are currently homing toward

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Trigger so it doesn't push anything or stop on walls
        col.isTrigger = true;
        col.sharedMaterial = null;
    }

    /// Initialize missile
    public void Init(Vector2 dir, int dmg, GameObject ownerGO)
    {
        owner = ownerGO;
        damage = dmg;

        // Aim the missile so the triangle tip points forward
        // (assuming the triangle tip points "up" in the sprite)
        transform.up = dir.normalized;

        rb.linearVelocity = dir.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (hasExploded) return;

        if (!enableHoming)
        {
            // just keep going straight at constant speed
            rb.linearVelocity = (Vector2)transform.up * speed;
            return;
        }

        // 1) Acquire or validate target
        UpdateTarget();

        // 2) Turn toward target (if any), otherwise keep straight
        Vector2 currentDir = transform.up;

        if (currentTarget != null)
        {
            Vector2 desiredDir = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;

            // Compute target rotation facing desiredDir
            Quaternion targetRot = Quaternion.LookRotation(Vector3.forward, desiredDir);

            // Rotate gradually toward target
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                turnSpeedDeg * Time.fixedDeltaTime
            );

            // Set velocity based on new facing
            rb.linearVelocity = (Vector2)transform.up * speed;
        }
        else
        {
            // No target in range: just fly straight
            rb.linearVelocity = currentDir * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        if (!other || other.gameObject == owner) return;

        // If missile touches ANY enemy → explode
        if (other.CompareTag("Enemy"))
        {
            Explode();
            return;
        }

        // Walls / objects: we IGNORE them completely (phase-through)
    }

    private void UpdateTarget()
    {
        // If we already have a target but it moved too far away, clear it
        if (currentTarget != null)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist > homingRadius || !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
            }
        }

        if (currentTarget != null) return; // keep current target

        // Find the nearest enemy within homingRadius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, homingRadius);

        float bestSqrDist = Mathf.Infinity;
        Transform best = null;

        foreach (var hit in hits)
        {
            if (!hit || !hit.CompareTag("Enemy")) continue;

            float sqrDist = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                best = hit.transform;
            }
        }

        currentTarget = best;
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Find all enemies via tag within radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Enemy"))
            {
                var enemy = hit.GetComponentInParent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject);
    }

    // Optional: draw explosion and homing radius in scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, homingRadius);
    }
}
