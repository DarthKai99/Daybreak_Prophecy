using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public enum EnemyType { Normal, Explosive, Ranged }

    [Header("Type")]
    public EnemyType type = EnemyType.Normal;

    [Header("Common Stats")]
    [SerializeField] private int hp = 2;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float chaseRange = 20f;
    [SerializeField] private int touchDamage = 1;             // Normal contact tick
    [SerializeField] private float touchDamageCooldown = 0.5f;

    [Header("Explosive")]
    [SerializeField] private float explodeRange = 1.2f;       // (visual/useful for gizmo; explosion triggers on touch)
    [SerializeField] private int explodeDamage = 4;
    [SerializeField] private float explodeRadius = 2.5f;
    [SerializeField] private bool explodeOnShot = true;       // explode immediately if damaged (before dying)
    [SerializeField] private LayerMask explodeHitMask;        // include Player + Enemy layers

    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float desiredRange = 6f;
    [SerializeField] private float keepRangeSlack = 1.5f;
    [SerializeField] private float fireCooldown = 1.0f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileSpeed = 10f;

    private Rigidbody2D rb;
    private Transform player;
    private float lastTouchDamageTime = -999f;
    private float nextShootTime = 0f;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        var stats = FindFirstObjectByType<PlayerStats>();
        if (stats) player = stats.transform;
    }

    void FixedUpdate()
    {
        if (isDead || !player)
        {
            SetVel(Vector2.zero);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > chaseRange)
        {
            SetVel(Vector2.zero);
            return; // idle if too far
        }

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;

        switch (type)
        {
            case EnemyType.Normal:
                // Chase and let contact tick damage handle hurting player
                SetVel(dir * moveSpeed);
                break;

            case EnemyType.Explosive:
                // Chase; explosion happens on contact or when shot (explodeOnShot)
                SetVel(dir * moveSpeed);
                break;

            case EnemyType.Ranged:
                float minR = desiredRange - keepRangeSlack;
                float maxR = desiredRange + keepRangeSlack;

                if (dist > maxR)        SetVel(dir * moveSpeed);    // move in
                else if (dist < minR)   SetVel(-dir * moveSpeed);   // back up
                else                    SetVel(Vector2.zero);       // hold

                if (Time.time >= nextShootTime)
                {
                    Shoot(dir);
                    nextShootTime = Time.time + fireCooldown;
                }
                break;
        }
    }

    // ===== Damage / Death =====
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        hp -= amount;

        // Explode immediately when shot (if enabled) and still alive
        if (type == EnemyType.Explosive && explodeOnShot && hp > 0)
        {
            Explode();
            return;
        }

        if (hp <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        var ps = FindFirstObjectByType<PlayerStats>();
        if (ps) ps.AddXP(5);

        var ts = FindFirstObjectByType<TimingSystem>();
        if (ts) ts.ReportEnemyKilled();

        Destroy(gameObject);
    }

    private void Explode()
    {
        if (isDead) return;
        isDead = true;

        // AoE damage to player + enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius, explodeHitMask);
        foreach (var h in hits)
        {
            if (!h) continue;

            var pStats = h.GetComponent<PlayerStats>();
            if (pStats) pStats.TakeDamage(explodeDamage);

            var e = h.GetComponent<Enemy>();
            if (e && e != this) e.TakeDamage(explodeDamage);
        }

        Destroy(gameObject);
    }

    // ===== Explosive: explode on first contact =====
    void OnCollisionEnter2D(Collision2D col)  { TryExplodeOnPlayer(col.gameObject); }
    void OnTriggerEnter2D(Collider2D col)     { TryExplodeOnPlayer(col.gameObject); }

    private void TryExplodeOnPlayer(GameObject other)
    {
        if (isDead) return;
        if (type != EnemyType.Explosive) return;

        var p = other.GetComponent<PlayerStats>();
        if (p == null) return;

        // Optional: small chip damage before boom
        // p.TakeDamage(touchDamage);

        Explode();
    }

    // ===== Normal: contact tick damage while overlapping =====
    void OnCollisionStay2D(Collision2D col) { TryTouchDamage(col.gameObject); }
    void OnTriggerStay2D(Collider2D col)    { TryTouchDamage(col.gameObject); }

    private void TryTouchDamage(GameObject other)
    {
        if (isDead) return;
        if (type != EnemyType.Normal) return; // only normals tick damage
        if (Time.time - lastTouchDamageTime < touchDamageCooldown) return;

        var p = other.GetComponent<PlayerStats>();
        if (!p) return;

        p.TakeDamage(touchDamage);
        lastTouchDamageTime = Time.time;
    }

    // ===== Shooting (Ranged) =====
    private void Shoot(Vector2 dir)
    {
        if (!projectilePrefab) return;

        Vector3 pos = transform.position + (Vector3)(dir * 0.6f);
        var go = Instantiate(projectilePrefab, pos, Quaternion.identity);

        var ep = go.GetComponent<EnemyProjectile>();
        if (!ep) ep = go.AddComponent<EnemyProjectile>();
        ep.Init(dir, projectileSpeed, projectileDamage, gameObject);
    }

    // Helper to support projects that don’t expose linearVelocity
    private void SetVel(Vector2 v)
    {
        // Use whichever your Unity supports:
        // rb.velocity = v;
        rb.linearVelocity = v; // if your version supports linearVelocity
    }

    // Debug gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (type == EnemyType.Explosive)
        {
            Gizmos.color = Color.red;    Gizmos.DrawWireSphere(transform.position, explodeRadius);
            Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, explodeRange);
        }

        if (type == EnemyType.Ranged)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, desiredRange - keepRangeSlack);
            Gizmos.DrawWireSphere(transform.position, desiredRange + keepRangeSlack);
        }
    }
}
