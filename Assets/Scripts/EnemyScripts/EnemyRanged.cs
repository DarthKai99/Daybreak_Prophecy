using UnityEngine;

/// Self-contained ranged enemy (no EnemyBase needed)
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]

public class EnemyRanged : MonoBehaviour
{
     [Header("Stats")]
    [SerializeField] private int hp = 3;                 // set ≥ 3 if you want 3 hits to kill
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float chaseRange = 20f;

    [Header("Keep Distance")]
    [SerializeField] private float desiredRange = 6f;    // target distance from player
    [SerializeField] private float rangeSlack = 1.5f;    // tolerance around desiredRange
    [SerializeField] private float backAwayBoost = 1.2f; // move a bit faster when backing up

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab; // prefab with EnemyProjectile on it
    [SerializeField] private float fireCooldown = 1f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 10f;

    [Header("Damage Gate")]
    [SerializeField] private bool onlyProjectileDamage = true; // ignore non-projectile damage by default

    [Header("Spawning / Collisions")]
    [Tooltip("Layers considered solid for projectile spawning (e.g., Default, Ground, Walls). Exclude Enemy/EnemyProjectile layers.")]
    [SerializeField] private LayerMask spawnBlockers = ~0; // default to everything; tune in Inspector

    private Rigidbody2D rb;
    private Transform player;
    private bool isDead = false;
    private float nextShootTime = 0f;

    // gate to allow only projectile-driven damage
    private bool _allowDamageThisFrame = false;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // find the player (expects PlayerStats on player)
        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats) player = stats.transform;
    }

    void FixedUpdate()
    {
        if (isDead || !player)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > chaseRange)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // keep distance band
        float minR = desiredRange - rangeSlack;
        float maxR = desiredRange + rangeSlack;

        Vector2 vel = Vector2.zero;
        if (dist > maxR)                      vel = dirToPlayer * moveSpeed;                       // move in
        else if (dist < minR)                 vel = -dirToPlayer * (moveSpeed * backAwayBoost);    // back up
        else                                  vel = Vector2.zero;                                  // hold

        rb.linearVelocity = vel;

        // shooting
        if (Time.time >= nextShootTime)
        {
            Shoot(dirToPlayer);
            nextShootTime = Time.time + fireCooldown;
        }
    }

    void Shoot(Vector2 dirToPlayer)
    {
        if (!projectilePrefab) return;

        float spawnOffset = 0.8f; // a bit in front to avoid self-hit
        Vector2 start = transform.position;
        Vector2 dir = dirToPlayer.normalized;

        // Determine a safe radius based on projectile's CircleCollider2D if present
        float radius = 0.1f;
        var cc = projectilePrefab.GetComponent<CircleCollider2D>();
        if (cc)
        {
            float scale = Mathf.Max(projectilePrefab.transform.localScale.x, projectilePrefab.transform.localScale.y);
            radius = Mathf.Max(0.05f, cc.radius * scale);
        }

        // Check the path to the intended spawn point (respect layer mask)
        RaycastHit2D hit = Physics2D.CircleCast(start, radius, dir, spawnOffset, spawnBlockers);
        Vector2 spawnPos = start + dir * spawnOffset;

        if (hit.collider != null && hit.collider.gameObject != gameObject)
        {
            // If blocked, place just before the wall (use radius + skin to avoid thin-collider overlap)
            const float skin = 0.05f;
            spawnPos = hit.point - dir * (radius + skin);
        }

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // ensure projectile has EnemyProjectile and fire it
        var ep = go.GetComponent<EnemyProjectile>();
        if (!ep) ep = go.AddComponent<EnemyProjectile>();
        ep.Init(dir, projectileSpeed, projectileDamage, gameObject);
    }

    // ===== Damage API =====

    // Call this from projectiles to apply damage even when onlyProjectileDamage = true
    public void ApplyProjectileDamage(int amount)
    {
        // Make sure the gate always closes even if something throws
        try
        {
            _allowDamageThisFrame = true;
            TakeDamage(amount);
        }
        finally
        {
            _allowDamageThisFrame = false;
        }
    }

    // PRIVATE: no other script should be able to call this directly
// Add near your other fields:
private bool touchingWallOrPlayer = false;

// Support BOTH collision and trigger contacts
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Player"))
    {
        touchingWallOrPlayer = true;
        Debug.Log($"[EnemyRanged] Began contact with {collision.collider.tag}");
    }
}

private void OnCollisionExit2D(Collision2D collision)
{
    if (collision.collider.CompareTag("Wall") || collision.collider.CompareTag("Player"))
    {
        touchingWallOrPlayer = false;
        Debug.Log($"[EnemyRanged] Ended contact with {collision.collider.tag}");
    }
}

private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Wall") || other.CompareTag("Player"))
    {
        touchingWallOrPlayer = true;
        Debug.Log($"[EnemyRanged] Began TRIGGER contact with {other.tag}");
    }
}

private void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Wall") || other.CompareTag("Player"))
    {
        touchingWallOrPlayer = false;
        Debug.Log($"[EnemyRanged] Ended TRIGGER contact with {other.tag}");
    }
}

// Replace your TakeDamage with this version:
private void TakeDamage(int amount)
{
    if (isDead) return;

    // Hard block while touching wall/player
    if (touchingWallOrPlayer)
    {
        Debug.Log($"[EnemyRanged] Ignored damage while touching Wall/Player (hp {hp}).");
        return;
    }

    if (onlyProjectileDamage && !_allowDamageThisFrame)
    {
        Debug.LogWarning($"[EnemyRanged] Blocked NON-projectile damage on {name}\n{System.Environment.StackTrace}");
        return;
    }

    Debug.Log($"[EnemyRanged] Took {amount} damage (hp {hp} -> {hp - amount}). allowGate={_allowDamageThisFrame}");
    hp -= amount;
    if (hp <= 0) Die();
}
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Award XP / notify UI if those exist
        var ps = FindFirstObjectByType<PlayerStats>();
        if (ps) ps.AddXP(5);

        var ts = FindFirstObjectByType<TimingSystem>();
        if (ts) ts.ReportEnemyKilled();

        Destroy(gameObject);
    }
}
